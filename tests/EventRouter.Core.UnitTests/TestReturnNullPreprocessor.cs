using System.Collections.Generic;

namespace EventRouter.Core.UnitTests
{
	class TestReturnNullPreprocessor : IRoutablePreprocessor<TestRoutable>
	{
		public bool OnEnqueueing { get; }

		public TestReturnNullPreprocessor(bool onEnqueueing = true)
		{
			this.OnEnqueueing = onEnqueueing;
		}

		public IEnumerable<TestRoutable> Process(TestRoutable routable)
		{
			return null;
		}
	}
}
