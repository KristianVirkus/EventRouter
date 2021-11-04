using System.Threading;

namespace EventRouter.Core
{
    /// <summary>
    /// Implements a queueable for queue synchronization based on a wait handle.
    /// </summary>
    public class QueueableWaitHandleSignal : IQueueable
    {
        /// <summary>
        /// Gets the assigned wait handle signalling the state.
        /// </summary>
        public WaitHandle Event => this._event.WaitHandle;

        ManualResetEventSlim _event { get; } = new ManualResetEventSlim(false);

        /// <summary>
        /// Gets whether the queueable was forwarded.
        /// </summary>
        public bool IsForwarded => this._event.IsSet;

        /// <summary>
        /// Triggers the signal.
        /// </summary>
        public void Signal()
        {
            this._event.Set();
        }
    }
}
