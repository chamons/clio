using System;
using Optional;

namespace clio
{
	public class SearchOptions
	{
		public bool IgnoreLowBugs { get; set; } = true;
		public bool OnlyListCommitsConsidered { get; set; } = false;
		public bool Explain { get; set; } = false;
		public Option<string> SingleCommit { get; set; } = Option.None<string> ();

		public Option<string> Starting { get; set; } = Option.None<string> ();
		public bool IncludeStarting { get; set; } = true;
		public Option<string> Ending { get; set; } = Option.None<string> ();


	}
}
