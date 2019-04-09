using EventRouter.Core;
using System;
using System.Threading.Tasks;

namespace EventRouter.SampleApp
{
	class Program
	{
		/// <summary>
		/// Gets some predefined message events.
		/// </summary>
		static readonly MessageEvent[] Events = new[]
			{
				new MessageEvent { Message = "Unimportant message." },
				new MessageEvent { Message = "Important message." },
			};

		static async Task Main(string[] args)
		{
			// Create configuration.
			var config = new HubConfigurationBuilder<MessageEvent>()
							.AddRouter(new ConsoleRouter())
							.AddPreprocessor(FilterPreprocessor<MessageEvent>
								.Allow(new Func<MessageEvent, bool>[] { e => e.Message.StartsWith("Important") }))
							.Build();

			// Create and configure event hub.
			var hub = new Hub<MessageEvent>();
			await hub.ReconfigureAsync(config, default);

			// Have events be forwarded to the configured routers.
			hub.Forward(Events);

			// Wait a second to allow concurrent event distribution mechanisms to complete.
			await Task.Delay(1000);
		}
	}
}
