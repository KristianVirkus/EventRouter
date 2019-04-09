using System;
using System.Collections.Generic;
using System.Linq;

namespace EventRouter.Core
{
	public class HubConfiguration<T>
		where T : IRoutable
	{
		/// <summary>
		/// Gets the routers.
		/// </summary>
		public IEnumerable<IRouter<T>> Routers { get; }

		/// <summary>
		/// Gets the preprocessors.
		/// </summary>
		public IEnumerable<IRoutablePreprocessor<T>> Preprocessors { get; }

		/// <summary>
		/// Gets the maximum length of the queue for routables.
		/// </summary>
		public int MaximumRoutablesQueueLength { get; }

		/// <summary>
		/// Gets the maximum number of routables to forward at once.
		/// </summary>
		public int MaximumRoutablesForwardingCount { get; }

		/// <summary>
		/// Gets the time to wait for more routables to be forwarded
		/// before actually starting to forward any routables.
		/// </summary>
		public TimeSpan WaitForMoreRoutablesForwardingDelay { get; }

		/// <summary>
		/// Initializes a new instance of the <see cref="HubConfiguration{T}"/> class.
		/// </summary>
		/// <param name="routers">The routers. Must not be null, but may be an empty
		///		list.</param>
		/// <param name="preprocessors">The preprocessors. Must not be null, but may be
		///		an empty list.</param>
		/// <param name="maximumRoutablesQueueLength">Maximum queue length for routables.
		///		Must be greater than zero.</param>
		///	<param name="maximumRoutablesForwardingCount">Maximum number of routables
		///		to forward in a single router invocation.</param>
		/// <param name="waitForMoreRoutablesForwardingDelay">The time to wait for
		///		even more routables coming in when forwarding to the routers.</param>
		public HubConfiguration(IEnumerable<IRouter<T>> routers,
			IEnumerable<IRoutablePreprocessor<T>> preprocessors,
			int maximumRoutablesQueueLength,
			int maximumRoutablesForwardingCount,
			TimeSpan waitForMoreRoutablesForwardingDelay)
		{
			this.Routers = routers?.ToList() ?? throw new ArgumentNullException(nameof(routers));
			this.Preprocessors = preprocessors?.ToList() ?? throw new ArgumentNullException(nameof(preprocessors));

			if (maximumRoutablesQueueLength <= 0)
				throw new ArgumentOutOfRangeException(ExceptionTexts.MaximumRoutablesQueueLengthOutOfRange, nameof(maximumRoutablesQueueLength));
			this.MaximumRoutablesQueueLength = maximumRoutablesQueueLength;

			if (maximumRoutablesForwardingCount <= 0)
				throw new ArgumentOutOfRangeException(ExceptionTexts.MaximumRoutablesForwardingCountOutOfRange, nameof(maximumRoutablesForwardingCount));
			this.MaximumRoutablesForwardingCount = maximumRoutablesForwardingCount;

			if (waitForMoreRoutablesForwardingDelay.Ticks < 0)
				throw new ArgumentOutOfRangeException(ExceptionTexts.WaitForMoreRoutablesForwardingDelayOutOfRange, nameof(waitForMoreRoutablesForwardingDelay));
			this.WaitForMoreRoutablesForwardingDelay = waitForMoreRoutablesForwardingDelay;
		}
	}
}
