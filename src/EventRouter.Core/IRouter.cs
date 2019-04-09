using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EventRouter.Core
{
	/// <summary>
	/// Common interface of all routers.
	/// </summary>
	/// <typeparam name="T">The routable event information type.</typeparam>
	public interface IRouter<T>
		where T: IRoutable
	{
		/// <summary>
		/// Starts the router.
		/// </summary>
		/// <param name="cancellationToken">The cancellation token to
		///		cancel starting the router. The instance's state might be
		///		undefined if canceled.</param>
		///	<exception cref="OperationCanceledException">Thrown if this
		///		operation has already been canceled via
		///		<paramref name="cancellationToken"/>.</exception>
		Task StartAsync(CancellationToken cancellationToken);

		/// <summary>
		/// Stops the router.
		/// </summary>
		/// <param name="cancellationToken">The cancellation token to
		///		cancel stopping the router. The instance's state might be
		///		undefined if canceled.</param>
		///	<exception cref="OperationCanceledException">Thrown if this
		///		operation has already been canceled via
		///		<paramref name="cancellationToken"/>.</exception>
		Task StopAsync(CancellationToken cancellationToken);

		/// <summary>
		/// Forwards routable information via this instance.
		/// </summary>
		/// <param name="routables">The routable information.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="ArgumentNullException">Thrown if
		///		<paramref name="routables"/> is null.</exception>
		///	<exception cref="OperationCanceledException">Thrown if
		///		<paramref name="cancellationToken"/> has been canceled.</exception>
		Task ForwardAsync(IEnumerable<T> routables, CancellationToken cancellationToken);
	}
}
