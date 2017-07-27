using System;
using System.IO;
using Optional;

namespace clio
{
	public class SearchOptions
	{
		public string OutputPath { get; set; } = Path.Combine (System.Environment.CurrentDirectory, "ReleaseNotes.md");
		public Option<string> Template { get; set; } = Option.None<string> ();

		public bool IgnoreLowBugs { get; set; } = true;
		public bool Explain { get; set; } = false;
		public Option<string> SingleCommit { get; set; } = Option.None<string> ();

		public Option<string> Starting { get; set; } = Option.None<string> ();
		public bool IncludeStarting { get; set; } = true;
		public Option<string> Ending { get; set; } = Option.None<string> ();
	}
}
