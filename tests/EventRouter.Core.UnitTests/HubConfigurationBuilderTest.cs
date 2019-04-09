using FluentAssertions;
using NUnit.Framework;
using System;

namespace EventRouter.Core.UnitTests
{
	class HubConfigurationBuilderTest
	{
		#region Constructors

		[Test]
		public void Constructor_Should_SetProperties()
		{
			var builder = new HubConfigurationBuilder<TestRoutable>();
			builder.Routers.Should().NotBeNull();
			builder.Preprocessors.Should().NotBeNull();
			builder.MaximumRoutableQueueLength.Should().NotBe(0);
			builder.MaximumRoutablesForwardingCount.Should().NotBe(0);
			builder.WaitForMoreRoutablesForwardingDelay.Should().NotBe(TimeSpan.Zero);
		}

		#endregion

		#region Build

		[Test]
		public void Build_Should_CreateHubConfiguration()
		{
			var router1 = new TestRouter<TestRoutable>();
			var preprocessor1 = new TestReturnNullPreprocessor();
			var builder = new HubConfigurationBuilder<TestRoutable>();
			builder.Routers.Add(router1);
			builder.Preprocessors.Add(preprocessor1);
			builder.MaximumRoutableQueueLength = 1;
			builder.MaximumRoutablesForwardingCount = 2;
			builder.WaitForMoreRoutablesForwardingDelay = TimeSpan.FromMilliseconds(3);

			var hubConfiguration = builder.Build();

			hubConfiguration.Routers.Should().Contain(router1);
			hubConfiguration.Preprocessors.Should().Contain(preprocessor1);
			hubConfiguration.MaximumRoutablesQueueLength.Should().Be(1);
			hubConfiguration.MaximumRoutablesForwardingCount.Should().Be(2);
			hubConfiguration.WaitForMoreRoutablesForwardingDelay.Should().Be(TimeSpan.FromMilliseconds(3));
		}

		#endregion

		#region Extension methods

		[Test]
		public void AddRouterNull_ShouldThrow_ArgumentNullException()
		{
			var builder = new HubConfigurationBuilder<TestRoutable>();
			Assert.Throws<ArgumentNullException>(() => HubConfigurationBuilderExtensions.AddRouter(builder, null));
		}

		[Test]
		public void AddRouter_Should_Succeed()
		{
			var router1 = new TestRouter<TestRoutable>();
			var builder = new HubConfigurationBuilder<TestRoutable>();
			HubConfigurationBuilderExtensions.AddRouter(builder, router1);
			builder.Routers.Should().Contain(router1);
		}

		[Test]
		public void AddPreprocessorNull_ShouldThrow_ArgumentNullException()
		{
			var builder = new HubConfigurationBuilder<TestRoutable>();
			Assert.Throws<ArgumentNullException>(() => HubConfigurationBuilderExtensions.AddPreprocessor(builder, null));
		}

		[Test]
		public void AddPreprocessor_Should_Succeed()
		{
			var preprocessor1 = new TestReturnNullPreprocessor();
			var builder = new HubConfigurationBuilder<TestRoutable>();
			HubConfigurationBuilderExtensions.AddPreprocessor<TestRoutable>(builder, preprocessor1);
			builder.Preprocessors.Should().Contain(preprocessor1);
		}

		[Test]
		public void SetMaximumRoutableQueueLength_Should_SetProperty()
		{
			var builder = new HubConfigurationBuilder<TestRoutable>();
			HubConfigurationBuilderExtensions.SetMaximumRoutableQueueLength<TestRoutable>(builder, 1);
			builder.MaximumRoutableQueueLength.Should().Be(1);
		}

		[Test]
		public void SetMaximumRoutablesForwardingCount_Should_SetProperty()
		{
			var builder = new HubConfigurationBuilder<TestRoutable>();
			HubConfigurationBuilderExtensions.SetMaximumRoutablesForwardingCount<TestRoutable>(builder, 1);
			builder.MaximumRoutablesForwardingCount.Should().Be(1);
		}

		[Test]
		public void SetWaitForMoreRoutablesForwardingDelay_Should_SetProperty()
		{
			var builder = new HubConfigurationBuilder<TestRoutable>();
			HubConfigurationBuilderExtensions.SetWaitForMoreRoutablesForwardingDelay<TestRoutable>(builder, TimeSpan.FromMilliseconds(1));
			builder.WaitForMoreRoutablesForwardingDelay.Should().Be(TimeSpan.FromMilliseconds(1));
		}

		#endregion
	}
}
