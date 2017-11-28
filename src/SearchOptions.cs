﻿using System;
using System.IO;
using Optional;

namespace clio
{
	public enum BugzillaLevel
	{
		Public,
		Private,
		Disable
	}

	public class SearchOptions
	{
		public BugzillaLevel Bugzilla { get; set; } = BugzillaLevel.Public;

		public bool IgnoreLowBugs { get; set; } = true;
		public bool SortBugs { get; set; } = true;
		public bool AdditionalBugInfo { get; set; } = false;
		public bool Submodules { get; set; } = false;

		public bool Explain { get; set; } = false;

		int ExplainIndent = 0;
		public void IndentExplain () => ExplainIndent += 1;
		public void DeindentExplain () => ExplainIndent -= 1;

		public void PrintExplain (string s)
		{
			if (Explain)
				Console.WriteLine (new string ('\t', ExplainIndent) + s);
		}
	}

	public class SearchRange
	{
		public Option<string> SingleCommit { get; set; } = Option.None<string> ();
		public Option<string> Oldest { get; set; } = Option.None<string> ();
		public Option<string> OldestBranch { get; set; } = Option.None<string> ();
		public bool IncludeOldest { get; set; } = true;
		public Option<string> Newest { get; set; } = Option.None<string> ();
	}
}
