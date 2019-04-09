using System.Collections.Generic;

namespace EventRouter.Core
{
	/// <summary>
	/// Common interface of all routable preprocessors.
	/// </summary>
	public interface IRoutablePreprocessor<T>
		where T : IRoutable
	{
		/// <summary>
		/// Gets whether the preprocessor needs to be executed on a routable
		/// event information right before enqueueing it for asynchronous
		/// routing (true.) Otherwise the routable preprocessor gets invoked
		/// right before routing the event information.
		/// </summary>
		bool OnEnqueueing { get; }

		/// <summary>
		/// Processes a <paramref name="routable"/>. This may drop items
		/// completely, replace them or add additional items.
		/// </summary>
		/// <param name="routable">The routable item.</param>
		/// <returns>List of replacement routable items, or null if
		///		the original <paramref name="routable"/> is to be kept.</returns>
		IEnumerable<T> Process(T routable);
	}
}
