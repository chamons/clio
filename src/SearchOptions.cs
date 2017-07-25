using System;
using Optional;

namespace clio
{
	public class SearchOptions
	{
		public bool IgnoreLowBugs { get; set; } = true;
		public Option<string> StartingHash { get; set; } = Option.None<string> ();
		public Option<string> EndingHash { get; set; } = Option.None<string> ();
	}
}
