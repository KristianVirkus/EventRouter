using FluentAssertions;
using NUnit.Framework;
using System;
using System.Linq;

namespace EventRouter.Core.UnitTests
{
	class FilterPreprocessorTest
	{
		static readonly Func<TestRoutable, bool>[] CatchingConditions;
		static readonly Func<TestRoutable, bool>[] NotCatchingConditions;
		static readonly Func<TestRoutable, bool>[] NoConditions;

		static FilterPreprocessorTest()
		{
			CatchingConditions = new Func<TestRoutable, bool>[] { (_r) => true };
			NotCatchingConditions = new Func<TestRoutable, bool>[] { (_r) => false };
			NoConditions = new Func<TestRoutable, bool>[0];
		}

		[Test]
		public void Constructor_Should_SetProperties()
		{
			var filterProcessor = new FilterPreprocessor<TestRoutable>(NoConditions, NoConditions, onEnqueueing: true);
			filterProcessor.OnEnqueueing.Should().BeTrue();
		}

		[Test]
		public void AllowConditionsNull_ShouldThrow_ArgumentNullException()
		{
			Assert.Throws<ArgumentNullException>(() =>
			{
				new FilterPreprocessor<TestRoutable>(null, CatchingConditions);
			});
		}

		[Test]
		public void BlockConditionsNull_ShouldThrow_ArgumentNullException()
		{
			Assert.Throws<ArgumentNullException>(() =>
			{
				new FilterPreprocessor<TestRoutable>(CatchingConditions, null);
			});
		}

		[Test]
		public void ProcessRoutableWithCatchingAllowCondition_ShouldReturn_Routable()
		{
			new FilterPreprocessor<TestRoutable>(CatchingConditions, NoConditions).Process(new TestRoutable()).Count().Should().Be(1);
		}

		[Test]
		public void ProcessRoutableWithNotCatchingAllowCondition_ShouldReturn_Routable()
		{
			new FilterPreprocessor<TestRoutable>(NotCatchingConditions, NoConditions).Process(new TestRoutable()).Count().Should().Be(0);
		}

		[Test]
		public void ProcessRoutableWithCatchingBlockCondition_ShouldReturn_Routable()
		{
			new FilterPreprocessor<TestRoutable>(NoConditions, CatchingConditions).Process(new TestRoutable()).Count().Should().Be(0);
		}

		[Test]
		public void ProcessRoutableWithNotCatchingBlockCondition_ShouldReturn_Routable()
		{
			new FilterPreprocessor<TestRoutable>(NoConditions, NotCatchingConditions).Process(new TestRoutable()).Count().Should().Be(1);
		}

		[Test]
		public void ProcessRoutableWithCatchingAllowAndBlockConditions_ShouldReturn_Routable()
		{
			new FilterPreprocessor<TestRoutable>(CatchingConditions, CatchingConditions).Process(new TestRoutable()).Count().Should().Be(0);
		}

		[Test]
		public void ProcessRoutableWithNotCatchingAllowAndBlockConditions_ShouldReturn_Routable()
		{
			new FilterPreprocessor<TestRoutable>(NotCatchingConditions, NotCatchingConditions).Process(new TestRoutable()).Count().Should().Be(0);
		}

		[Test]
		public void ProcessRoutableWithNoConditions_ShouldReturn_Routable()
		{
			new FilterPreprocessor<TestRoutable>(NoConditions, NoConditions).Process(new TestRoutable()).Count().Should().Be(1);
		}

		[Test]
		public void AllowNull_ShouldThrow_ArgumentNullException()
		{
			Assert.Throws<ArgumentNullException>(() => { FilterPreprocessor<TestRoutable>.Allow(null); });
		}

		[Test]
		public void BlockNull_ShouldThrow_ArgumentNullException()
		{
			Assert.Throws<ArgumentNullException>(() => { FilterPreprocessor<TestRoutable>.Block(null); });
		}

		[Test]
		public void AllowWithCatchingCondition_ShouldReturn_Routable()
		{
			FilterPreprocessor<TestRoutable>.Allow(CatchingConditions).Process(new TestRoutable()).Count().Should().Be(1);
		}

		[Test]
		public void BlockWithNotCatchingCondition_ShouldReturn_Routable()
		{
			FilterPreprocessor<TestRoutable>.Block(NotCatchingConditions).Process(new TestRoutable()).Count().Should().Be(1);
		}
	}
}
