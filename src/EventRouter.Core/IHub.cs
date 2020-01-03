using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EventRouter.Core
{
    /// <summary>
    /// Common interface of all event hubs.
    /// </summary>
    /// <typeparam name="T">The routable event information type.</typeparam>
    public interface IHub<T>
        where T : IRoutable
    {
        /// <summary>
        /// Gets the current configuration.
        /// </summary>
        HubConfiguration<T> Configuration { get; }

        /// <summary>
        /// Gets whether the hub is currently running.
        /// </summary>
        bool IsRunning { get; }

        /// <summary>
        /// Reconfigures the hub. The hub can be stopped by using
        /// null for <paramref name="configuration"/>.
        /// </summary>
        /// <param name="configuration">The new configuration or null
        ///		to stop the hub.</param>
        /// <param name="cancellationToken">The cancellation token to
        ///		cancel reconfiguring. The instance's state might be
        ///		undefined if canceled.</param>
        ///	<exception cref="TaskCanceledException">Thrown if
        ///		<paramref name="cancellationToken"/> is canceled.</exception>
        Task ReconfigureAsync(HubConfiguration<T> configuration, CancellationToken cancellationToken);

        /// <summary>
        /// Forwards a routable object.
        /// </summary>
        /// <param name="routables">The routable object.</param>
        /// <exception cref="ArgumentNullException">Thrown if
        ///		<paramref name="routables"/> is null.</exception>
        ///	<exception cref="InvalidOperationException">Thrown if
        ///		the hub is not configured.</exception>
        void Forward(IEnumerable<T> routables);
    }
}
