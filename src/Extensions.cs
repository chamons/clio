using System;
using System.Collections.Generic;

namespace clio
{
	public static class EnumerableExtensions
	{
		public static IEnumerable<T> Yield<T> (this T item)
		{
			yield return item;
		}
	}

	public static class StringExtensions
	{
		public static IEnumerable<string> SplitLines (this string item)
		{
			return item.Split (new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
		}
	}
}