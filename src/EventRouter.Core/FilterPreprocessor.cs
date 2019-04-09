using System;
using System.Collections.Generic;

namespace EventRouter.Core
{
	/// <summary>
	/// Implements filtering based on allowing and blocking conditions.
	/// </summary>
	/// <typeparam name="T">The event type.</typeparam>
	public class FilterPreprocessor<T> : IRoutablePreprocessor<T>
		where T : IRoutable
	{
		static readonly IEnumerable<T> NoRoutables;

		readonly IEnumerable<Func<T, bool>> allowConditions = new List<Func<T, bool>>();
		readonly IEnumerable<Func<T, bool>> blockConditions = new List<Func<T, bool>>();

		/// <summary>
		/// Gets whether the filtering should take place upon enqueueing (true) or
		/// at the time the event is just about to get routed (false.) In the latter
		/// case, the router must care about this.
		/// </summary>
		public bool OnEnqueueing { get; }

		static FilterPreprocessor()
		{
			NoRoutables = new List<T>();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="FilterPreprocessor"/> class.
		/// </summary>
		/// <param name="allowConditions">The allowing conditions. All conditions
		///		must be fulfiled to not block the event.</param>
		/// <param name="blockConditions">The blocking conditions.</param>
		/// <param name="onEnqueueing">true if filtering should occur upon
		///		enqueueing, false if filtering should occur at the time
		///		of forwarding an event to all routers.</param>
		public FilterPreprocessor(IEnumerable<Func<T, bool>> allowConditions,
			IEnumerable<Func<T, bool>> blockConditions, bool onEnqueueing = true)
		{
			this.allowConditions = allowConditions ?? throw new ArgumentNullException(nameof(allowConditions));
			this.blockConditions = blockConditions ?? throw new ArgumentNullException(nameof(blockConditions));
			this.OnEnqueueing = onEnqueueing;
		}

		public IEnumerable<T> Process(T routable)
		{
			foreach (var condition in this.allowConditions)
			{
				if (!condition(routable)) return NoRoutables;
			}

			foreach (var condition in this.blockConditions)
			{
				if (condition(routable)) return NoRoutables;
			}

			return new T[] { routable };
		}

		/// <summary>
		/// Configures conditions which are required to be met in order to have a
		/// routable event be forwarded.
		/// </summary>
		/// <typeparam name="T">The event type.</typeparam>
		/// <param name="conditions">The conditions required to be met.</param>
		/// <returns>The passed on routable events.</returns>
		public static FilterPreprocessor<T> Allow(IEnumerable<Func<T, bool>> conditions)
		{
			return new FilterPreprocessor<T>(conditions, new Func<T, bool>[0]);
		}

		/// <summary>
		/// Configures conditions which if met immediately filter routable events.
		/// </summary>
		/// <typeparam name="T">The event type.</typeparam>
		/// <param name="conditions">The conditions filtering routable events if met.</param>
		/// <returns>The passed on routable events.</returns>
		public static FilterPreprocessor<T> Block(IEnumerable<Func<T, bool>> conditions)
		{
			return new FilterPreprocessor<T>(new Func<T, bool>[0], conditions);
		}
	}
}
