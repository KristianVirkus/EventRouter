using System;
using System.Threading;

namespace EventRouter.Core.UnitTests
{
    public class TestQueueableSignal : IQueueable
    {
        public ManualResetEventSlim Event { get; } = new ManualResetEventSlim(false);

        /// <inheritdoc/>
        public bool IsForwarded => this.Event.IsSet;

        public Action SignalCallback { get; set; }

        /// <inheritdoc/>
        public void SignalForwarded()
        {
            this.SignalCallback?.Invoke();
            this.Event.Set();
        }
    }
}
