using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EventRouter.Core
{
    /// <summary>
    /// Common interface of all flushable event hubs.
    /// </summary>
    /// <typeparam name="T">The routable event information type.</typeparam>
    public interface IFlushableHub<T> : IHub<T>
        where T : IRoutable
    {
        /// <summary>
        /// Flushes all routers' buffers where possible.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token to
        ///		cancel flushing.</param>
        ///	<exception cref="TaskCanceledException">Thrown if
        ///		<paramref name="cancellationToken"/> is canceled.</exception>
        /// <returns>The list of routers which failed to flush their
        ///     buffers with the appropriate exception. Can be null if
        ///     no errors occurred.</returns>
        Task<IEnumerable<(IFlushableRouter<T> Router, Exception Exception)>> FlushAsync(CancellationToken cancellationToken);
    }
}
