using System;
using System.Threading;
using System.Threading.Tasks;

namespace EventRouter.Core
{
    /// <summary>
    /// Common interface of all flushable routers.
    /// </summary>
    /// <typeparam name="T">The routable event information type.</typeparam>
    public interface IFlushableRouter<T> : IRouter<T>
        where T: IRoutable
    {
        /// <summary>
        /// Flushes the router's buffers.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token to
        ///		cancel flushing.</param>
        ///	<exception cref="TaskCanceledException">Thrown if
        ///		<paramref name="cancellationToken"/> is canceled.</exception>
        ///	<exception cref="Exception">Thrown if anything failed during
        ///	    flushing the router's buffers.</exception>
        /// <returns>The list of routers which failed to flush their
        ///     buffers with the appropriate exception.</returns>
        Task FlushAsync(CancellationToken cancellationToken);
    }
}
