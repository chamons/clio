using System;
using System.Collections.Generic;

namespace Clio.Utilities 
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

		public static IEnumerable<string> Split (this string item, char character)
		{
			return item.Split (new char [] { character }, 1, StringSplitOptions.None);
		}
	}
}