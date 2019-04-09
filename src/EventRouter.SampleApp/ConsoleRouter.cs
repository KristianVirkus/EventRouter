using EventRouter.Core;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EventRouter.SampleApp
{
	/// <summary>
	/// Implements an event router writing all events to the console.
	/// </summary>
	class ConsoleRouter : IRouter<MessageEvent>
	{
		public Task ForwardAsync(IEnumerable<MessageEvent> routables, CancellationToken cancellationToken)
		{
			foreach (var routable in routables)
			{
				if (cancellationToken.IsCancellationRequested) break;
				Console.WriteLine(routable.Message);
			}

			return Task.CompletedTask;
		}

		public Task StartAsync(CancellationToken cancellationToken)
		{
			// TODO Start background actions like opening connections if required.
			return Task.CompletedTask;
		}

		public Task StopAsync(CancellationToken cancellationToken)
		{
			// TODO Stop background actions.
			return Task.CompletedTask;
		}
	}
}
