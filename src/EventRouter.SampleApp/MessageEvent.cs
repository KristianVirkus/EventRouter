using EventRouter.Core;

namespace EventRouter.SampleApp
{
	/// <summary>
	/// Represents a message event.
	/// </summary>
	class MessageEvent : IRoutable
	{
		public string Message { get; set; }
	}
}
