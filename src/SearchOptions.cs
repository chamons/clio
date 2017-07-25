using System;
using Optional;

namespace clio
{
	public class SearchOptions
	{
		public bool IgnoreLowBugs { get; set; } = true;

		public Option<string> Starting { get; set; } = Option.None<string> ();
		public bool IncludeStarting { get; set; } = true;
		public Option<string> Ending { get; set; } = Option.None<string> ();

		public bool OnlyListCommitsConsidered = false;
	}
}
