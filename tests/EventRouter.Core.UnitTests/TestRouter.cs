using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EventRouter.Core.UnitTests
{
	class TestRouter<T> : IRouter<T>
		where T : IRoutable
	{
		SemaphoreSlim sync = new SemaphoreSlim(1);

		public bool IsRunning { get; private set; }
		public Action<IEnumerable<T>> ForwardCallback { get; set; }

		public async Task ForwardAsync(IEnumerable<T> routables, CancellationToken cancellationToken)
		{
			await this.sync.WaitAsync(cancellationToken);
			try
			{
				if (this.IsRunning) this.ForwardCallback?.Invoke(routables);
			}
			finally
			{
				sync.Release();
			}
		}

		public async Task StartAsync(CancellationToken cancellationToken)
		{
			await this.sync.WaitAsync(cancellationToken);
			try
			{
				this.IsRunning = true;
			}
			finally
			{
				this.sync.Release();
			}
		}

		public async Task StopAsync(CancellationToken cancellationToken)
		{
			await this.sync.WaitAsync(cancellationToken);
			try
			{
				this.IsRunning = false;
			}
			finally
			{
				this.sync.Release();
			}
		}
	}
}
