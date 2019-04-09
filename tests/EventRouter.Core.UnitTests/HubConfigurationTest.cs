using NUnit.Framework;
using FluentAssertions;
using System;
using System.Collections.Generic;

namespace EventRouter.Core.UnitTests
{
	public class HubConfigurationTest
	{
		const int MaximumRoutablesQueueLengthDefault = 100;
		const int MaximumRoutablesForwardingCountDefault = 100;
		static readonly TimeSpan WaitForMoreRoutablesForwardingDelayDefault;

		static HubConfigurationTest()
		{
			WaitForMoreRoutablesForwardingDelayDefault = TimeSpan.FromMilliseconds(200);
		}

		void prepareConfiguration(IEnumerable<IRouter<TestRoutable>> routers,
			IEnumerable<IRoutablePreprocessor<TestRoutable>> preprocessors,
			out HubConfiguration<TestRoutable> configuration)
		{
			configuration = new HubConfiguration<TestRoutable>(
				routers,
				preprocessors,
				MaximumRoutablesQueueLengthDefault,
				MaximumRoutablesForwardingCountDefault,
				WaitForMoreRoutablesForwardingDelayDefault);
		}

		#region Constructors

		[Test]
		public void Constructor_Should_SetProperties()
		{
			var router1 = new TestRouter<TestRoutable>();
			var routers = new IRouter<TestRoutable>[] { router1 };
			var preprocessor1 = new TestReturnNullPreprocessor();
			var preprocessors = new IRoutablePreprocessor<TestRoutable>[] { preprocessor1 };
			this.prepareConfiguration(routers, preprocessors, out var configuration);
			configuration.Routers.Should().Contain(routers);
			configuration.Preprocessors.Should().Contain(preprocessor1);
			configuration.MaximumRoutablesQueueLength.Should().Be(MaximumRoutablesQueueLengthDefault);
		}

		[Test]
		public void ConstructorRoutersNull_ShouldThrow_ArgumentNullException()
		{
			var preprocessors = new IRoutablePreprocessor<TestRoutable>[0];
			Assert.Throws<ArgumentNullException>(() => this.prepareConfiguration(null, preprocessors, out var _));
		}

		[Test]
		public void ConstructorPreprocessorsNull_ShouldThrow_ArgumentNullException()
		{
			var routers = new IRouter<TestRoutable>[0];
			Assert.Throws<ArgumentNullException>(() => this.prepareConfiguration(routers, null, out var _));
		}

		[Test]
		public void ConstructorMaximumRoutablesQueueLengthZero_ShouldThrow_ArgumentOutOfRangeException()
		{
			var routers = new IRouter<TestRoutable>[0];
			var preprocessors = new IRoutablePreprocessor<TestRoutable>[0];
			Assert.Throws<ArgumentOutOfRangeException>(() => new HubConfiguration<TestRoutable>(
				routers,
				preprocessors,
				0,
				MaximumRoutablesForwardingCountDefault,
				WaitForMoreRoutablesForwardingDelayDefault));
		}

		[Test]
		public void ConstructorMaximumRoutablesQueueLengthNegative_ShouldThrow_ArgumentOutOfRangeException()
		{
			var routers = new IRouter<TestRoutable>[0];
			var preprocessors = new IRoutablePreprocessor<TestRoutable>[0];
			Assert.Throws<ArgumentOutOfRangeException>(() => new HubConfiguration<TestRoutable>(
				routers,
				preprocessors,
				-1,
				MaximumRoutablesForwardingCountDefault,
				WaitForMoreRoutablesForwardingDelayDefault));
		}

		[Test]
		public void ConstructorMaximumRoutablesForwardingCountZero_ShouldThrow_ArgumentOutOfRangeException()
		{
			var routers = new IRouter<TestRoutable>[0];
			var preprocessors = new IRoutablePreprocessor<TestRoutable>[0];
			Assert.Throws<ArgumentOutOfRangeException>(() => new HubConfiguration<TestRoutable>(
				routers,
				preprocessors,
				MaximumRoutablesQueueLengthDefault,
				0,
				WaitForMoreRoutablesForwardingDelayDefault));
		}

		[Test]
		public void ConstructorMaximumRoutablesForwardingCountNegative_ShouldThrow_ArgumentOutOfRangeException()
		{
			var routers = new IRouter<TestRoutable>[0];
			var preprocessors = new IRoutablePreprocessor<TestRoutable>[0];
			Assert.Throws<ArgumentOutOfRangeException>(() => new HubConfiguration<TestRoutable>(
				routers,
				preprocessors,
				MaximumRoutablesQueueLengthDefault,
				-1,
				WaitForMoreRoutablesForwardingDelayDefault));
		}

		[Test]
		public void ConstructorWaitForMoreRoutablesForwardingDelayZero_Should_Succeed()
		{
			var routers = new IRouter<TestRoutable>[0];
			var preprocessors = new IRoutablePreprocessor<TestRoutable>[0];
			var hubConfiguration = new HubConfiguration<TestRoutable>(
				routers,
				preprocessors,
				MaximumRoutablesQueueLengthDefault,
				MaximumRoutablesForwardingCountDefault,
				TimeSpan.Zero);
			hubConfiguration.WaitForMoreRoutablesForwardingDelay.Should().Be(TimeSpan.Zero);
		}

		[Test]
		public void ConstructorWaitForMoreRoutablesForwardingDelayNegative_ShouldThrow_ArgumentOutOfRangeException()
		{
			var routers = new IRouter<TestRoutable>[0];
			var preprocessors = new IRoutablePreprocessor<TestRoutable>[0];
			Assert.Throws<ArgumentOutOfRangeException>(() => new HubConfiguration<TestRoutable>(
				routers,
				preprocessors,
				MaximumRoutablesQueueLengthDefault,
				MaximumRoutablesForwardingCountDefault,
				TimeSpan.FromMilliseconds(-1)));
		}

		#endregion
	}
}