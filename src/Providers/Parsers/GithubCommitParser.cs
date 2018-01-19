using System;
using System.Text.RegularExpressions;
using clio.Model;

namespace clio.Providers.Parsers
{
	public class GithubCommitParser : BaseCommitParser
	{
		static Regex[] AllRegex = { };

		public static readonly GithubCommitParser Instance = new GithubCommitParser (AllRegex);

		GithubCommitParser (Regex[] bugRegexes) : base (IssueSource.GitHub, bugRegexes, DefaultLikelyBugRegexes)
		{
		}

		protected override bool ValidateBugNumber (int bugNumber)
		{
			throw new NotImplementedException ();
		}
	}
}
