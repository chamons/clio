using System;
using Optional;

namespace clio
{
	public enum BugzillaLevel
	{
		/// <summary>
		/// Only bugzilla bugs that are public are validated. Authentication is not required to validate.
		/// </summary>
		Public,

		/// <summary>
		/// Authentication is required to validate private bugs, instructs validator to authenticate and
		/// locate all bugs so that private issues can be verified
		/// </summary>
		Private,

		/// <summary>
		/// Bugzilla bugs are not validated and will retain their default confidence as determined by the commit parser
		/// </summary>
		Disable
	}

	public enum VstsLevel
	{
		/// <summary>
		/// Authentication is required to validate private bugs, instructs validator to authenticate and
		/// locate all bugs so that private issues can be verified
		/// </summary>
		Enable,

		/// <summary>
		/// Vsts bugs are not validated and will retain their default confidence as determined by the commit parser
		/// </summary>
		Disable
	}

	public class SearchOptions
	{
		/// <summary>
		/// Gets or sets a value indicating whether to ignore bugzilla issues or not
		/// </summary>
		public bool IgnoreBugzilla { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether to ignore VSTS issues or not
		/// </summary>
		public bool IgnoreVsts { get; set; }

		/// <summary>
		/// Gets or sets the level to which bugzilla issues are validated. The default is public issues.
		/// </summary>
		public BugzillaLevel Bugzilla { get; set; } = BugzillaLevel.Public;

		/// <summary>
		/// Gets or sets the level to which VSTS issues are validated. The default is disabled.
		/// </summary>
		public VstsLevel Vsts { get; set; } = VstsLevel.Disable;

		/// <summary>
		/// Gets or sets the PAT for accessing VSTS issues
		/// </summary>
		public string VstsPAT { get; set; }

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
