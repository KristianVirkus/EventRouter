using System;

namespace EventRouter.Core
{
    /// <summary>
    /// Implements an event that can be enqueued.
    /// </summary>
    /// <typeparam name="T">The routable event information type.</typeparam>
    class QueueableEvent<T> : IQueueable
        where T : IRoutable
    {
        /// <summary>
        /// Gets the routable event.
        /// </summary>
        public T Event { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="QueueableEvent{T}"/> class.
        /// </summary>
        /// <param name="evt">The event.</param>
        /// <exception cref="ArgumentNullException">Thrown, if
        ///     <paramref name="evt"/> is null.</exception>
        public QueueableEvent(T evt)
        {
            this.Event = evt ?? throw new ArgumentNullException(nameof(evt));
        }
    }
}
