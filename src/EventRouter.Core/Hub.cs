﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EventRouter.Core
{
    /// <summary>
    /// Implements a hub to distribute events via routers.
    /// </summary>
    /// <typeparam name="T">The routable event information type.</typeparam>
    public class Hub<T> : IHub<T>, IFlushableHub<T>
        where T : IRoutable
    {
        #region Constants

        const int InitialQueueCapacity = 1000;

        #endregion

        #region Fields

        readonly SemaphoreSlim sync = new SemaphoreSlim(1, 1);

        readonly SemaphoreSlim enqueueSync = new SemaphoreSlim(1, 1);
        BlockingCollection<IQueueable> queue = new BlockingCollection<IQueueable>(new ConcurrentQueue<IQueueable>(), InitialQueueCapacity);
        CancellationTokenSource forwardingTaskCancellationTokenSource;

        Task forwardingTask;

        #endregion

        #region Properties

        public HubConfiguration<T> Configuration { get; private set; }

        public bool IsRunning => (this.Configuration != null);

        #endregion

        public virtual async Task ReconfigureAsync(HubConfiguration<T> configuration, CancellationToken cancellationToken)
        {
            await this.sync.WaitAsync(cancellationToken);
            try
            {
                // Cancel task.
                this.forwardingTaskCancellationTokenSource?.Cancel();

                if (cancellationToken.IsCancellationRequested) return;

                // Stop routers and wait until all have finished routing.
                if (this.Configuration != null)
                {
                    Task.WaitAll(this.Configuration.Routers.Select(r => r.StopAsync(cancellationToken)).ToArray(), cancellationToken);
                }

                if (cancellationToken.IsCancellationRequested) return;

                // Apply new configuration.
                this.Configuration = configuration;

                if (configuration == null)
                {
                    // Keep queue to reuse its contents when reconfiguring again.
                    this.forwardingTaskCancellationTokenSource = null;
                    this.forwardingTask = null;
                }
                else
                {
                    Task.WaitAll(this.Configuration.Routers.Select(r => r.StartAsync(cancellationToken)).ToArray(), cancellationToken);

                    // Create new queue with configured capacity while reusing left-over
                    // queue contents from previous configuration or from the
                    // not-configured state.
                    this.queue = new BlockingCollection<IQueueable>(
                        new ConcurrentQueue<IQueueable>(this.queue.ToArray() ?? Array.Empty<IQueueable>()),
                        configuration.MaximumRoutablesQueueLength);

                    var cts = new CancellationTokenSource();
                    this.forwardingTaskCancellationTokenSource = cts;

                    this.forwardingTask = new Task(() =>
                    {
                        try
                        {
                            this.processQueue(configuration, queue, cts.Token);
                        }
                        catch (OperationCanceledException)
                        {
                            throw;
                        }
                        catch
                        {
                        }
                    }, cts.Token, TaskCreationOptions.LongRunning);
                    this.forwardingTask.Start();
                }
            }
            catch
            {
                throw;
            }
            finally
            {
                this.sync.Release();
            }
        }

        public void Forward(IEnumerable<T> routables)
        {
            if (routables == null) throw new ArgumentNullException(nameof(routables));

            var config = this.Configuration;

            var forwardRoutables = routables.Where(r => r != null).ToList();

            // If currently not configured, just enqueue.
            if (config != null)
            {
                foreach (var preprocessor in config.Preprocessors.Where(p => p.OnEnqueueing))
                {
                    int i = 0;
                    while (i < forwardRoutables.Count)
                    {
                        var routable = forwardRoutables[i];
                        var replacementRoutables = preprocessor.Process(routable);
                        if (replacementRoutables != null)
                        {
                            // Remove original item first.
                            forwardRoutables.RemoveAt(i);

                            if (replacementRoutables.Any())
                            {
                                // Insert replacement items.
                                forwardRoutables.InsertRange(i, replacementRoutables);
                                i += replacementRoutables.Count();
                            }
                        }
                        else
                        {
                            // Keep routable item.
                            ++i;
                        }
                    }
                }
            }

            if (forwardRoutables.Any())
            {
                this.enqueue(forwardRoutables.Select(r => new QueueableEvent<T>(evt: r)));
            }
        }

        void enqueue(IEnumerable<IQueueable> queueables)
        {
            // Use synchronisation to keep order of routables being enqueued concurrently.
            this.enqueueSync.Wait();
            try
            {
                if (this.queue is BlockingCollection<IQueueable> queue)
                {
                    foreach (var queueable in queueables)
                    {
                        queue.TryAdd(queueable); // TODO Log if queue full.
                    }
                }
            }
            finally
            {
                this.enqueueSync.Release();
            }
        }

        void processQueue(HubConfiguration<T> configuration, BlockingCollection<IQueueable> queue,
            CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var list = new List<IQueueable>();

                    // Get next routable to forward.
                    var queueable = queue.Take(cancellationToken);
                    list.Add(queueable);

                    // Get even more routables to forward within the configured time frame.
                    var tryTakeStarted = DateTime.UtcNow;
                    while ((!cancellationToken.IsCancellationRequested)
                        && (list.Count < configuration.MaximumRoutablesForwardingCount)
                        && (queue.TryTake(out queueable, TimeSpan.FromTicks(Math.Max(0, (tryTakeStarted.Add(configuration.WaitForMoreRoutablesForwardingDelay) - DateTime.UtcNow).Ticks)))))
                    {
                        list.Add(queueable);
                    }

                    if (cancellationToken.IsCancellationRequested) return;

                    // Apply after-enqueueing preprocessors now.
                    // TODO Test.
                    foreach (var preprocessor in configuration.Preprocessors.Where(p => !p.OnEnqueueing))
                    {
                        int i = 0;
                        while (i < list.Count)
                        {
                            var preprocessedRoutable = list[i];
                            if (preprocessedRoutable is QueueableEvent<T> queueableEvent)
                            {
                                var replacementRoutables = preprocessor.Process(queueableEvent.Event);
                                if (replacementRoutables != null)
                                {
                                    // Remove original item first.
                                    list.RemoveAt(i);

                                    if (replacementRoutables.Any())
                                    {
                                        // Insert replacement items.
                                        list.InsertRange(i, replacementRoutables.Select(r => new QueueableEvent<T>(evt: r)).ToArray());
                                        i += replacementRoutables.Count();
                                    }
                                }
                                else
                                {
                                    // Keep item.
                                    ++i;
                                }
                            }
                            else
                            {
                                // Not a routable. Keep item.
                                ++i;
                            }
                        }
                    }

                    if (cancellationToken.IsCancellationRequested) return;

                    if (list.Any())
                    {
                        foreach (var router in configuration.Routers)
                        {
                            try
                            {
                                router.ForwardAsync(list.OfType<QueueableEvent<T>>().Select(q => q.Event), default).GetAwaiter().GetResult();
                            }
                            catch
                            {
                                // TODO Log.
                            }
                        }

                        foreach (var r in list.OfType<QueueableWaitHandleSignal>())
                        {
                            try
                            {
                                r.Signal();
                            }
                            catch
                            {
                                // TODO Log.
                            }
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    return;
                }
                catch
                {
                    // TODO Log.
                }
            }
        }

        #region IFlushableHub<T> implementation

        /// <summary>
        /// Flushes all routers' buffers where possible.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token to
        ///		cancel flushing.</param>
        ///	<exception cref="TaskCanceledException">Thrown if
        ///		<paramref name="cancellationToken"/> is canceled.</exception>
        /// <returns>The list of routers which failed to flush their
        ///     buffers with the appropriate exception. Can be null if
        ///     no errors occurred.</returns>
        public async Task<IEnumerable<(IFlushableRouter<T> Router, Exception Exception)>> FlushAsync(
            CancellationToken cancellationToken)
        {
            var results = new List<(IFlushableRouter<T> Router, Exception Exception)>();

            IEnumerable<IFlushableRouter<T>> routers = null;
            await this.sync.WaitAsync(cancellationToken);
            try
            {
                if (this.Configuration != null)
                {
                    routers = this.Configuration.Routers.OfType<IFlushableRouter<T>>();
                }
            }
            finally
            {
                this.sync.Release();
            }

            if (routers == null) return null;

            var signallable = new QueueableWaitHandleSignal();
            this.enqueue(new IQueueable[] { signallable });

            // Wait until synchronization marker has been processed and thus all previous
            // events have been forwarded to the configured routers.
            if (WaitHandle.WaitAny(new[] { signallable.Event, cancellationToken.WaitHandle }) == 1)
                throw new TaskCanceledException();

            // Now it can be waited for routers to finish flushing as it is guaranteed that
            // they have received all previous events.
            // Flush routers outside of the lock to avoid dead-locks in
            // case any of the routers interacts with this logfile.
            foreach (var router in routers)
            {
                cancellationToken.ThrowIfCancellationRequested();

                try
                {
                    await router.FlushAsync(cancellationToken: cancellationToken);
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    results.Add((Router: router, Exception: ex));
                }
            }

            return results;
        }

        #endregion
    }
}
