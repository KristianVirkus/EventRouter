using System.Collections.Generic;

namespace EventRouter.Core.UnitTests
{
	static class ObjectExtensions
	{
		public static IEnumerable<T> AsEnumerable<T>(this T self)
		{
			return new T[] { self };
		}
	}
}
