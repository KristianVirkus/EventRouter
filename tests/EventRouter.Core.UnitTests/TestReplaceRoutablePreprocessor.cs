using System.Collections.Generic;

namespace EventRouter.Core.UnitTests
{
	class TestReplaceRoutablePreprocessor<T> : IRoutablePreprocessor<T>
		where T : IRoutable
	{
		public bool OnEnqueueing { get; }

		public TestReplaceRoutablePreprocessor(bool onEnqueueing = true)
		{
			this.OnEnqueueing = onEnqueueing;
		}

		public IEnumerable<T> Process(T routable)
		{
			var results = new T[] { (T)(IRoutable)(new TestRoutable()), (T)(IRoutable)new TestRoutable() };
			return results;
		}
	}
}
