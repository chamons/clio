using System;
using System.Text.RegularExpressions;
using clio.Model;

namespace clio.Providers.Parsers
{
	public class GithubCommitParser : BaseCommitParser
	{
		static Regex GithubIssue = new Regex (@"htt.*?:\/\/github\.com\/[\w-.]+\/[\w-.]+\/issues\/(\d*)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

		static Regex[] AllRegex = { GithubIssue };

		public static readonly GithubCommitParser Instance = new GithubCommitParser (AllRegex);

		// We do _not_ want DefaultLikelyBugRegexes unlike github / VSTS
		GithubCommitParser (Regex[] bugRegexes) : base (IssueSource.GitHub, bugRegexes, new Regex [] {})
		{
		}

		protected override bool ValidateBugNumber (int bugNumber)
		{
			return bugNumber < 100000;
		}
	}
}
