using System;
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
		public string OutputPath { get; set; } = Path.Combine (System.Environment.CurrentDirectory, "ReleaseNotes.md");
		public Option<string> Template { get; set; } = Option.None<string> ();

		public BugzillaLevel Bugzilla { get; set; } = BugzillaLevel.Public;

		public bool IgnoreLowBugs { get; set; } = true;
		public bool SortBugs { get; set; } = true;

		public bool Explain { get; set; } = false;
		public Option<string> SingleCommit { get; set; } = Option.None<string> ();

		public Option<string> Oldest { get; set; } = Option.None<string> ();
		public bool IncludeOldest { get; set; } = true;
		public Option<string> Newest { get; set; } = Option.None<string> ();
	}
}
