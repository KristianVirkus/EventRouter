using FluentAssertions;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EventRouter.Core.UnitTests
{
    class HubTest
    {
        const int MaximumRoutablesQueueLengthDefault = 100;
        const int MaximumRoutablesForwardingCountDefault = 100;
        static readonly TimeSpan WaitForMoreRoutablesForwardingDelayDefault;

        static HubTest()
        {
            WaitForMoreRoutablesForwardingDelayDefault = TimeSpan.FromMilliseconds(200);
        }

        void prepare(out TestRouter<TestRoutable> router, out HubConfiguration<TestRoutable> hubConfiguration)
        {
            router = new TestRouter<TestRoutable>();
            var routers = new IRouter<TestRoutable>[] { router };

            var preprocessors = new IRoutablePreprocessor<TestRoutable>[0];

            hubConfiguration = new HubConfiguration<TestRoutable>(
                routers,
                preprocessors,
                MaximumRoutablesQueueLengthDefault,
                MaximumRoutablesForwardingCountDefault,
                WaitForMoreRoutablesForwardingDelayDefault);
        }

        async Task<Hub<T>> runHub<T>(HubConfiguration<T> hubConfiguration)
            where T : IRoutable
        {
            var hub = new Hub<T>();
            await hub.ReconfigureAsync(hubConfiguration, default);
            return hub;
        }

        #region Reconfigure

        [Test]
        public async Task Reconfigure_Should_StoreConfigurationAndSetRunning()
        {
            var routers = new IRouter<TestRoutable>[0];
            var preprocessors = new IRoutablePreprocessor<TestRoutable>[0];
            var hubConfiguration = new HubConfiguration<TestRoutable>(
                routers,
                preprocessors,
                MaximumRoutablesQueueLengthDefault,
                MaximumRoutablesForwardingCountDefault,
                WaitForMoreRoutablesForwardingDelayDefault);
            var hub = new Hub<TestRoutable>();
            hub.IsRunning.Should().BeFalse();
            await hub.ReconfigureAsync(hubConfiguration, default);
            hub.Configuration.Should().BeSameAs(hubConfiguration);
            hub.IsRunning.Should().BeTrue();
        }

        [Test]
        public async Task ReconfigureNull_Should_RemoveConfigurationAndResetRunning()
        {
            this.prepare(out var router, out var hubConfiguration);
            var hub = await this.runHub(hubConfiguration);
            await hub.ReconfigureAsync(null, default);
            hub.Configuration.Should().BeNull();
            hub.IsRunning.Should().BeFalse();
        }

        [Test]
        public async Task ReconfigureNull_Should_WaitForBusyRouters()
        {
            var stage1Event = new ManualResetEventSlim();
            var stage2Event = new ManualResetEventSlim();
            var router1 = new TestRouter<TestRoutable>
            {
                ForwardCallback = (_routables) =>
                {
                    stage1Event.Set();
                    Thread.Sleep(TimeSpan.FromMilliseconds(1000));
                    stage2Event.Set();
                }
            };
            var hubConfiguration = new HubConfiguration<TestRoutable>(
                new IRouter<TestRoutable>[] { router1 },
                new IRoutablePreprocessor<TestRoutable>[0],
                MaximumRoutablesQueueLengthDefault,
                MaximumRoutablesForwardingCountDefault,
                WaitForMoreRoutablesForwardingDelayDefault);

            var hub = await this.runHub(hubConfiguration);
            var routable1 = new TestRoutable();
            hub.Forward(routable1.AsEnumerable());

            stage1Event.Wait();
            Assert.Throws<OperationCanceledException>(() =>
            {
                hub.ReconfigureAsync(null, new CancellationTokenSource(TimeSpan.FromMilliseconds(500)).Token).GetAwaiter().GetResult();
            });
            stage2Event.Wait();
            await hub.ReconfigureAsync(null, new CancellationTokenSource(TimeSpan.FromMilliseconds(1000)).Token);
        }

        [Test]
        public async Task ReconfigureNull_Should_StopForwarding()
        {
            var forwarded1Event = new ManualResetEventSlim();
            var forwarded2Event = new ManualResetEventSlim();
            var router1 = new TestRouter<TestRoutable>
            {
                ForwardCallback = (_routables) =>
                {
                    if (forwarded1Event.IsSet)
                        forwarded2Event.Set();
                    else
                        forwarded1Event.Set();
                }
            };
            var hubConfiguration = new HubConfiguration<TestRoutable>(
                new IRouter<TestRoutable>[] { router1 },
                new IRoutablePreprocessor<TestRoutable>[0],
                MaximumRoutablesQueueLengthDefault,
                MaximumRoutablesForwardingCountDefault,
                WaitForMoreRoutablesForwardingDelayDefault);

            var hub = await this.runHub(hubConfiguration);

            var routable1 = new TestRoutable();
            hub.Forward(routable1.AsEnumerable());
            forwarded1Event.Wait(TimeSpan.FromMilliseconds(500)).Should().BeTrue();

            await hub.ReconfigureAsync(null, default);

            var routable2 = new TestRoutable();
            hub.Forward(routable2.AsEnumerable());
            forwarded2Event.Wait(TimeSpan.FromMilliseconds(500)).Should().BeFalse();
        }

        [Test]
        public async Task ForwardWhileNotConfigured_Should_DelayEvents()
        {
            var forwarded1Event = new ManualResetEventSlim();
            var forwarded2Event = new ManualResetEventSlim();
            var router1 = new TestRouter<TestRoutable>
            {
                ForwardCallback = (_routables) =>
                {
                    if (forwarded1Event.IsSet)
                        forwarded2Event.Set();
                    else
                        forwarded1Event.Set();
                }
            };
            var hubConfiguration = new HubConfiguration<TestRoutable>(
                new IRouter<TestRoutable>[] { router1 },
                new IRoutablePreprocessor<TestRoutable>[0],
                MaximumRoutablesQueueLengthDefault,
                MaximumRoutablesForwardingCountDefault,
                WaitForMoreRoutablesForwardingDelayDefault);

            var hub = new Hub<TestRoutable>();

            var routable1 = new TestRoutable();
            hub.Forward(routable1.AsEnumerable());
            forwarded1Event.Wait(TimeSpan.FromMilliseconds(500)).Should().BeFalse();

            await hub.ReconfigureAsync(hubConfiguration, default);

            forwarded1Event.Wait(TimeSpan.FromMilliseconds(500)).Should().BeTrue();

            await hub.ReconfigureAsync(null, default);

            var routable2 = new TestRoutable();
            hub.Forward(routable2.AsEnumerable());
            forwarded2Event.Wait(TimeSpan.FromMilliseconds(500)).Should().BeFalse();

            await hub.ReconfigureAsync(hubConfiguration, default);

            forwarded2Event.Wait(TimeSpan.FromMilliseconds(500)).Should().BeTrue();
        }

        [Test]
        public async Task Reconfigure_Should_StartRouting()
        {
            this.prepare(out var router, out var hubConfiguration);
            var hub = await this.runHub(hubConfiguration);
            var routed = new ManualResetEventSlim(false);
            router.ForwardCallback = (_routable) => { routed.Set(); };
            var routable1 = new TestRoutable();
            hub.Forward(routable1.AsEnumerable());
            routed.Wait(TimeSpan.FromMilliseconds(500)).Should().BeTrue();
        }

        [Test]
        public void ReconfigureWithCancelledToken_ShouldThrow_TaskCanceledException()
        {
            Assert.ThrowsAsync<TaskCanceledException>(async () =>
            {
                var cts = new CancellationTokenSource();
                cts.Cancel();
                await new Hub<TestRoutable>().ReconfigureAsync(
                    new HubConfiguration<TestRoutable>(
                        new IRouter<TestRoutable>[0],
                        new IRoutablePreprocessor<TestRoutable>[0],
                        MaximumRoutablesQueueLengthDefault,
                        MaximumRoutablesForwardingCountDefault,
                        WaitForMoreRoutablesForwardingDelayDefault),
                    cts.Token);
            });
        }

        #endregion

        #region Forward

        [Test]
        public async Task ForwardToMultipleRouters_Should_IgnoreExceptionInRouters()
        {
            var router1 = new TestRouter<TestRoutable>
            {
                ForwardCallback = (_routables) => { throw new InvalidOperationException(); }
            };
            var routed2 = new ManualResetEventSlim(false);
            var router2 = new TestRouter<TestRoutable>
            {
                ForwardCallback = (_routables) => { routed2.Set(); }
            };
            var hubConfiguration = new HubConfiguration<TestRoutable>(
                new IRouter<TestRoutable>[] { router1, router2 },
                new IRoutablePreprocessor<TestRoutable>[0],
                MaximumRoutablesQueueLengthDefault,
                MaximumRoutablesForwardingCountDefault,
                WaitForMoreRoutablesForwardingDelayDefault);

            var hub = await this.runHub(hubConfiguration);
            var routable1 = new TestRoutable();
            hub.Forward(routable1.AsEnumerable());
            routed2.Wait(TimeSpan.FromMilliseconds(500)).Should().BeTrue();
        }

        [Test]
        public async Task ForwardNull_ShouldThrow_ArgumentNullException()
        {
            this.prepare(out var router, out var hubConfiguration);
            var hub = await this.runHub(hubConfiguration);
            Assert.Throws<ArgumentNullException>(() => hub.Forward(null));
        }

        [Test]
        public async Task ForwardWithNullRoutables_Should_IgnoreNullRoutables()
        {
            this.prepare(out var router, out var hubConfiguration);
            var hub = await this.runHub(hubConfiguration);
            hub.Forward(new TestRoutable[] { null });
        }

        #endregion

        #region Preprocessors

        [Test]
        public async Task ForwardWithReplacementPreprocessorOnEnqueueing_Should_ReplaceEvents()
        {
            var routed1 = new ManualResetEventSlim(false);
            IEnumerable<TestRoutable> routables1 = null;
            var router1 = new TestRouter<TestRoutable>()
            {
                ForwardCallback = (_routables) => { routables1 = _routables; routed1.Set(); }
            };
            var routers = new IRouter<TestRoutable>[] { router1 };
            var preprocessors = new IRoutablePreprocessor<TestRoutable>[] { new TestReplaceRoutablePreprocessor<TestRoutable>() };
            var hubConfiguration = new HubConfiguration<TestRoutable>(
                routers,
                preprocessors,
                MaximumRoutablesQueueLengthDefault,
                MaximumRoutablesForwardingCountDefault,
                WaitForMoreRoutablesForwardingDelayDefault);
            var hub = new Hub<TestRoutable>();
            await hub.ReconfigureAsync(hubConfiguration, default);
            hub.Forward(new[] { new TestRoutable() });
            routed1.Wait(TimeSpan.FromMilliseconds(500)).Should().BeTrue();
            routables1.Count().Should().Be(2);
        }

        [Test]
        public async Task ForwardWithPreprocessorReturnNullOnEnqueueing_Should_LetThroughEvents()
        {
            var routed1 = new ManualResetEventSlim(false);
            IEnumerable<TestRoutable> routables1 = null;
            var router1 = new TestRouter<TestRoutable>()
            {
                ForwardCallback = (_routables) => { routables1 = _routables; routed1.Set(); }
            };
            var routers = new IRouter<TestRoutable>[] { router1 };
            var preprocessors = new IRoutablePreprocessor<TestRoutable>[] { new TestReturnNullPreprocessor() };
            var hubConfiguration = new HubConfiguration<TestRoutable>(
                routers,
                preprocessors,
                MaximumRoutablesQueueLengthDefault,
                MaximumRoutablesForwardingCountDefault,
                WaitForMoreRoutablesForwardingDelayDefault);
            var hub = new Hub<TestRoutable>();
            await hub.ReconfigureAsync(hubConfiguration, default);
            hub.Forward(new[] { new TestRoutable() });
            routed1.Wait(TimeSpan.FromMilliseconds(500)).Should().BeTrue();
            routables1.Count().Should().Be(1);
        }

        [Test]
        public async Task ForwardWithPreprocessorReturnEmptyListOnEnqueueing_Should_BlockAllEvents()
        {
            var routed1 = new ManualResetEventSlim(false);
            IEnumerable<TestRoutable> routables1 = null;
            var router1 = new TestRouter<TestRoutable>()
            {
                ForwardCallback = (_routables) => { routables1 = _routables; routed1.Set(); }
            };
            var routers = new IRouter<TestRoutable>[] { router1 };
            var preprocessors = new IRoutablePreprocessor<TestRoutable>[] { new TestRemoveRoutablePreprocessor() };
            var hubConfiguration = new HubConfiguration<TestRoutable>(
                routers,
                preprocessors,
                MaximumRoutablesQueueLengthDefault,
                MaximumRoutablesForwardingCountDefault,
                WaitForMoreRoutablesForwardingDelayDefault);
            var hub = new Hub<TestRoutable>();
            await hub.ReconfigureAsync(hubConfiguration, default);
            hub.Forward(new[] { new TestRoutable() });
            routed1.Wait(TimeSpan.FromMilliseconds(500)).Should().BeFalse();
            routables1.Should().BeNull();
        }

        [Test]
        public async Task ForwardWithReplacementPreprocessorOnRouting_Should_ReplaceEvents()
        {
            var routed1 = new ManualResetEventSlim(false);
            IEnumerable<TestRoutable> routables1 = null;
            var router1 = new TestRouter<TestRoutable>()
            {
                ForwardCallback = (_routables) => { routables1 = _routables; routed1.Set(); }
            };
            var routers = new IRouter<TestRoutable>[] { router1 };
            var preprocessors = new IRoutablePreprocessor<TestRoutable>[] { new TestReplaceRoutablePreprocessor<TestRoutable>(onEnqueueing: false) };
            var hubConfiguration = new HubConfiguration<TestRoutable>(
                routers,
                preprocessors,
                MaximumRoutablesQueueLengthDefault,
                MaximumRoutablesForwardingCountDefault,
                WaitForMoreRoutablesForwardingDelayDefault);
            var hub = new Hub<TestRoutable>();
            await hub.ReconfigureAsync(hubConfiguration, default);
            hub.Forward(new[] { new TestRoutable() });
            routed1.Wait(TimeSpan.FromMilliseconds(500)).Should().BeTrue();
            routables1.Count().Should().Be(2);
        }

        [Test]
        public async Task ForwardWithPreprocessorReturnNullOnRouting_Should_LetThroughEvents()
        {
            var routed1 = new ManualResetEventSlim(false);
            IEnumerable<TestRoutable> routables1 = null;
            var router1 = new TestRouter<TestRoutable>()
            {
                ForwardCallback = (_routables) => { routables1 = _routables; routed1.Set(); }
            };
            var routers = new IRouter<TestRoutable>[] { router1 };
            var preprocessors = new IRoutablePreprocessor<TestRoutable>[] { new TestReturnNullPreprocessor(onEnqueueing: false) };
            var hubConfiguration = new HubConfiguration<TestRoutable>(
                routers,
                preprocessors,
                MaximumRoutablesQueueLengthDefault,
                MaximumRoutablesForwardingCountDefault,
                WaitForMoreRoutablesForwardingDelayDefault);
            var hub = new Hub<TestRoutable>();
            await hub.ReconfigureAsync(hubConfiguration, default);
            hub.Forward(new[] { new TestRoutable() });
            routed1.Wait(TimeSpan.FromMilliseconds(500)).Should().BeTrue();
            routables1.Count().Should().Be(1);
        }

        [Test]
        public async Task ForwardWithPreprocessorReturnEmptyListOnRouting_Should_BlockAllEvents()
        {
            var routed1 = new ManualResetEventSlim(false);
            IEnumerable<TestRoutable> routables1 = null;
            var router1 = new TestRouter<TestRoutable>()
            {
                ForwardCallback = (_routables) => { routables1 = _routables; routed1.Set(); }
            };
            var routers = new IRouter<TestRoutable>[] { router1 };
            var preprocessors = new IRoutablePreprocessor<TestRoutable>[] { new TestRemoveRoutablePreprocessor(onEnqueueing: false) };
            var hubConfiguration = new HubConfiguration<TestRoutable>(
                routers,
                preprocessors,
                MaximumRoutablesQueueLengthDefault,
                MaximumRoutablesForwardingCountDefault,
                WaitForMoreRoutablesForwardingDelayDefault);
            var hub = new Hub<TestRoutable>();
            await hub.ReconfigureAsync(hubConfiguration, default);
            hub.Forward(new[] { new TestRoutable() });
            routed1.Wait(TimeSpan.FromMilliseconds(500)).Should().BeFalse();
            routables1.Should().BeNull();
        }

        #endregion

        #region Flushing

        [Test]
        public async Task Flush_Should_InvokeFlushOnRouters()
        {
            // Arrange
            var router1Flushed = false;
            var router2Flushed = false;
            var router1 = Mock.Of<IFlushableRouter<TestRoutable>>();
            Mock.Get(router1)
                .Setup(m => m.FlushAsync(It.IsAny<CancellationToken>()))
                .Returns<CancellationToken>(_ =>
                {
                    router1Flushed = true;
                    return Task.CompletedTask;
                });
            var router2 = Mock.Of<IFlushableRouter<TestRoutable>>();
            Mock.Get(router2)
                .Setup(m => m.FlushAsync(It.IsAny<CancellationToken>()))
                .Returns<CancellationToken>(_ =>
                {
                    router2Flushed = true;
                    return Task.CompletedTask;
                });
            var hubConfiguration = new HubConfiguration<TestRoutable>(
                new IFlushableRouter<TestRoutable>[] { router1, router2 },
                new IRoutablePreprocessor<TestRoutable>[0],
                MaximumRoutablesQueueLengthDefault,
                MaximumRoutablesForwardingCountDefault,
                WaitForMoreRoutablesForwardingDelayDefault);

            // Act
            var hub = await this.runHub(hubConfiguration);
            var results = await hub.FlushAsync(default).ConfigureAwait(false);

            // Assert
            results.Should().BeEmpty();
            router1Flushed.Should().BeTrue();
            router2Flushed.Should().BeTrue();
        }

        [Test]
        public async Task FlushWithCancellationTokenAlreadyCanceled_ShouldThrow_TaskCanceledException()
        {
            // Arrange
            var router = Mock.Of<IRouter<TestRoutable>>();
            var hubConfiguration = new HubConfiguration<TestRoutable>(
                new IRouter<TestRoutable>[] { router },
                new IRoutablePreprocessor<TestRoutable>[0],
                MaximumRoutablesQueueLengthDefault,
                MaximumRoutablesForwardingCountDefault,
                WaitForMoreRoutablesForwardingDelayDefault);

            // Act
            var hub = await this.runHub(hubConfiguration);

            // Assert
            Assert.ThrowsAsync<TaskCanceledException>(
                async () => await hub.FlushAsync(new CancellationToken(true)).ConfigureAwait(false));
        }

        [Test]
        public async Task FlushWithExceptionInFirstRouter_Should_ReportExceptionButInvokeFlushOnSecondRouterAnyway()
        {
            // Arrange
            var router1Flushed = false;
            var router2Flushed = false;
            var exception1 = new Exception();
            var router1 = Mock.Of<IFlushableRouter<TestRoutable>>();
            Mock.Get(router1)
                .Setup(m => m.FlushAsync(It.IsAny<CancellationToken>()))
                .Returns<CancellationToken>(_ =>
                {
                    router1Flushed = true;
                    throw exception1;
                });
            var router2 = Mock.Of<IFlushableRouter<TestRoutable>>();
            Mock.Get(router2)
                .Setup(m => m.FlushAsync(It.IsAny<CancellationToken>()))
                .Returns<CancellationToken>(_ =>
                {
                    router2Flushed = true;
                    return Task.CompletedTask;
                });
            var hubConfiguration = new HubConfiguration<TestRoutable>(
                new IFlushableRouter<TestRoutable>[] { router1, router2 },
                new IRoutablePreprocessor<TestRoutable>[0],
                MaximumRoutablesQueueLengthDefault,
                MaximumRoutablesForwardingCountDefault,
                WaitForMoreRoutablesForwardingDelayDefault);

            // Act
            var hub = await this.runHub(hubConfiguration);
            var results = await hub.FlushAsync(default).ConfigureAwait(false);

            // Assert
            results.Single().Router.Should().BeSameAs(router1);
            results.Single().Exception.Should().BeSameAs(exception1);
            router1Flushed.Should().BeTrue();
            router2Flushed.Should().BeTrue();
        }

        [Test]
        public async Task FlushWithFirstRouterBlocking_Should_CompleteFlushingAnyway()
        {
            // Arrange
            var router1Flushed = new ManualResetEventSlim();
            var router2Flushed = new ManualResetEventSlim();
            var exception1 = new Exception();
            var router1 = Mock.Of<IFlushableRouter<TestRoutable>>();
            Mock.Get(router1)
                .Setup(m => m.FlushAsync(It.IsAny<CancellationToken>()))
                .Returns<CancellationToken>(async _cancellationToken =>
                {
                    await Task.Delay(TimeSpan.FromSeconds(2), _cancellationToken);
                    router1Flushed.Set();
                });
            var router2 = Mock.Of<IFlushableRouter<TestRoutable>>();
            Mock.Get(router2)
                .Setup(m => m.FlushAsync(It.IsAny<CancellationToken>()))
                .Returns<CancellationToken>(_ =>
                {
                    router2Flushed.Set();
                    return Task.CompletedTask;
                });
            var hubConfiguration = new HubConfiguration<TestRoutable>(
                new IFlushableRouter<TestRoutable>[] { router1, router2 },
                new IRoutablePreprocessor<TestRoutable>[0],
                MaximumRoutablesQueueLengthDefault,
                MaximumRoutablesForwardingCountDefault,
                WaitForMoreRoutablesForwardingDelayDefault);

            // Act
            var hub = await this.runHub(hubConfiguration);
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            var results = await hub.FlushAsync(default).ConfigureAwait(false);

            // Assert
            router2Flushed.Wait(TimeSpan.FromSeconds(1));
            router1Flushed.Wait(TimeSpan.FromSeconds(3));
            stopwatch.Stop();
            stopwatch.Elapsed.Should().BeCloseTo(TimeSpan.FromSeconds(2), TimeSpan.FromMilliseconds(500));
        }

        #endregion
    }
}
