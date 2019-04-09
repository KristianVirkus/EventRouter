using System;
using System.Collections.Generic;

namespace EventRouter.Core
{
	public class HubConfigurationBuilder<T>
		where T : IRoutable
	{
		const int MaximumRoutablesQueueLengthDefault = 100;
		const int MaximumRoutablesForwardingCountDefault = 100;
		static readonly TimeSpan WaitForMoreRoutablesForwardingDelayDefault = TimeSpan.FromMilliseconds(100);

		/// <summary>
		/// Gets the routers.
		/// </summary>
		public IList<IRouter<T>> Routers { get; }

		/// <summary>
		/// Gets the preprocessors.
		/// </summary>
		public IList<IRoutablePreprocessor<T>> Preprocessors { get; }

		/// <summary>
		/// Gets the maximum length of the queue for routables.
		/// </summary>
		public int MaximumRoutableQueueLength { get; set; }

		/// <summary>
		/// Gets the maximum number of routables to forward at once.
		/// </summary>
		public int MaximumRoutablesForwardingCount { get; set; }

		/// <summary>
		/// Gets the time to wait for more routables to be forwarded
		/// before actually starting to forward any routables.
		/// </summary>
		public TimeSpan WaitForMoreRoutablesForwardingDelay { get; set; }

		public HubConfigurationBuilder()
		{
			this.Routers = new List<IRouter<T>>();
			this.Preprocessors = new List<IRoutablePreprocessor<T>>();
			this.MaximumRoutableQueueLength = MaximumRoutablesQueueLengthDefault;
			this.MaximumRoutablesForwardingCount = MaximumRoutablesForwardingCountDefault;
			this.WaitForMoreRoutablesForwardingDelay = WaitForMoreRoutablesForwardingDelayDefault;
		}

		public virtual HubConfiguration<T> Build()
		{
			return new HubConfiguration<T>(
				this.Routers,
				this.Preprocessors,
				this.MaximumRoutableQueueLength,
				this.MaximumRoutablesForwardingCount,
				this.WaitForMoreRoutablesForwardingDelay);
		}
	}

	public static class HubConfigurationBuilderExtensions
	{
		/// <summary>
		/// Adds a router.
		/// </summary>
		/// <typeparam name="T">The event type.</typeparam>
		/// <param name="self">The configuration builder.</param>
		/// <param name="router">The router to add.</param>
		/// <returns>The same configuration builder instance to allow a fluent syntax.</returns>
		public static HubConfigurationBuilder<T> AddRouter<T>(this HubConfigurationBuilder<T> self, IRouter<T> router)
			where T : IRoutable
		{
			if (router == null) throw new ArgumentNullException(nameof(router));
			self.Routers.Add(router);
			return self;
		}

		/// <summary>
		/// Adds a preprocessor.
		/// </summary>
		/// <typeparam name="T">The event type.</typeparam>
		/// <param name="self">The configuration builder.</param>
		/// <param name="preprocessor">The preprocessor to add.</param>
		/// <returns>The same configuration builder instance to allow a fluent syntax.</returns>
		public static HubConfigurationBuilder<T> AddPreprocessor<T>(this HubConfigurationBuilder<T> self, IRoutablePreprocessor<T> preprocessor)
			where T : IRoutable
		{
			if (preprocessor == null) throw new ArgumentNullException(nameof(preprocessor));
			self.Preprocessors.Add(preprocessor);
			return self;
		}

		/// <summary>
		/// Sets the maximum length of the queue for routables.
		/// </summary>
		/// <typeparam name="T">The event type.</typeparam>
		/// <param name="self">The configuration builder.</param>
		/// <param name="value">The maximum queue length.</param>
		/// <returns>The same configuration builder instance to allow a fluent syntax.</returns>
		public static HubConfigurationBuilder<T> SetMaximumRoutableQueueLength<T>(this HubConfigurationBuilder<T> self, int value)
			where T : IRoutable
		{
			self.MaximumRoutableQueueLength = value;
			return self;
		}

		/// <summary>
		/// Set the maximum number of routables to forward at once.
		/// </summary>
		/// <typeparam name="T">The event type.</typeparam>
		/// <param name="self">The configuration builder.</param>
		/// <param name="value">The count.</param>
		/// <returns>The same configuration builder instance to allow a fluent syntax.</returns>
		public static HubConfigurationBuilder<T> SetMaximumRoutablesForwardingCount<T>(this HubConfigurationBuilder<T> self, int value)
			where T : IRoutable
		{
			self.MaximumRoutablesForwardingCount = value;
			return self;
		}

		/// <summary>
		/// Sets the time to wait for more routables to be forwarded
		/// before actually starting to forward any routables.
		/// </summary>
		/// <typeparam name="T">The event type.</typeparam>
		/// <param name="self">The configuration builder.</param>
		/// <param name="value">The delay.</param>
		/// <returns>The same configuration builder instance to allow a fluent syntax.</returns>
		public static HubConfigurationBuilder<T> SetWaitForMoreRoutablesForwardingDelay<T>(this HubConfigurationBuilder<T> self, TimeSpan value)
			where T : IRoutable
		{
			self.WaitForMoreRoutablesForwardingDelay = value;
			return self;
		}
	}
}
