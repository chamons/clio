using System;
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
		public bool AdditionalBugInfo { get; set; } = false;
		public bool SplitEnhancementBugs { get; set; } = true;
		public bool ValidateBugStatus { get; set; } = false;
		public string ExpectedTargetMilestone { get; set; }
		public bool Submodules { get; set; } = false;
	}

	public class SearchRange
	{
		public Option<string> Oldest { get; set; } = Option.None<string> ();
		public Option<string> OldestBranch { get; set; } = Option.None<string> ();
		public bool IncludeOldest { get; set; } = true;
		public Option<string> Newest { get; set; } = Option.None<string> ();
	}

	public static class Explain
	{
		static public bool Enabled;

		static int ExplainIndent = 0;
		static public void Indent () => ExplainIndent += 1;
		static public void Deindent () => ExplainIndent -= 1;

		static public void Print (string s)
		{
			if (Enabled)
				Console.WriteLine (new string ('\t', ExplainIndent) + s);
		}
	}
}
