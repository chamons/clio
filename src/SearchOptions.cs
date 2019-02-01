using System;
using System.Collections.Generic;
using Optional;

namespace clio
{
	public enum VstsLevel
	{
		// Authentication is required to validate private bugs, instructs validator to authenticate
		Enable,

		/// Vsts bugs are not validated and will retain their default confidence
		Disable
	}

	public class SearchOptions
	{
		public bool IgnoreVsts { get; set; } = true;
		public bool IgnoreGithub { get; set; }

		public VstsLevel Vsts { get; set; } = VstsLevel.Disable;
		public string GithubLocation { get; set; }

		public string GithubPAT { get; set; }
		public string VstsPAT { get; set; }

		public bool AdditionalBugInfo { get; set; } = false;
		public bool SplitEnhancementBugs { get; set; } = true;
		public bool CollectAuthors { get; set; } = false;
		public List<string> CommitsToIgnore { get; set; } = new List<string> ();
	}

	public interface ISearchRange {}

	public class HashSearchRange : ISearchRange
	{
		public string Oldest { get; set; }
		public string Newest { get; set; }
	}

	public class BranchSearchRange : ISearchRange
	{
		public string Base { get; set; }	
		public string Branch { get; set; }
	}

	public static class Explain
	{
		static public bool Enabled;

		static int ExplainIndent = 0;
		static public void Indent () => ExplainIndent += 1;
		static public void Deindent ()
		{
			ExplainIndent -= 1;
			if (ExplainIndent < 0)
				throw new InvalidOperationException ("Can not indent negative");
		}

		static public void Print (string s)
		{
			if (Enabled)
				Console.WriteLine (new string ('\t', ExplainIndent) + s);
		}
	}
}
