using System.Collections.Generic;

namespace EventRouter.Core.UnitTests
{
	class TestRemoveRoutablePreprocessor : IRoutablePreprocessor<TestRoutable>
	{
		public bool OnEnqueueing { get; }

		public TestRemoveRoutablePreprocessor(bool onEnqueueing = true)
		{
			this.OnEnqueueing = onEnqueueing;
		}

		public IEnumerable<TestRoutable> Process(TestRoutable routable)
		{
			return new TestRoutable[0];
		}
	}
}
