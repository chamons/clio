using System;
using System.Text.RegularExpressions;
using clio.Model;

namespace clio.Providers.Parsers
{
	public class GithubCommitParser : BaseCommitParser
	{
		static Regex GithubIssue = new Regex (@"htt.*?:\/\/github\.com\/([\w-.]+)\/([\w-.]+)\/issues\/(\d*)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
		static Regex GithubPulls = new Regex (@"htt.*?:\/\/github\.com\/([\w-.]+)\/([\w-.]+)\/pulls\/(\d*)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

		static Regex BuildPath = new Regex (@"obj/iPhone/Debug64", RegexOptions.Compiled | RegexOptions.IgnoreCase);

		static Regex[] AllRegex = { GithubIssue };

		public static readonly GithubCommitParser DefaultInstance = new GithubCommitParser ();
		public static GithubCommitParser Create (string repro) => new GithubCommitParser (repro);

		public string ExpectedRepo { get; } = "";

		public GithubCommitParser () : base (IssueSource.GitHub, AllRegex, DefaultLikelyBugRegexes)
		{
		}

		public GithubCommitParser (string expectedRepo) : base (IssueSource.GitHub, AllRegex, DefaultLikelyBugRegexes)
		{
			if (expectedRepo.Contains ("/"))
				expectedRepo = expectedRepo.Split ('/')[1];
			ExpectedRepo = expectedRepo;
		}

		protected override ValidationResults ValidateLine (string line, int bugNumber)
		{
			if (BuildPath.Match (line).Success) {
				Explain.Print ($"{line} appears to be build path not issue path, ignoring.");
				return ValidationResults.Ignore;
			}

			if (!String.IsNullOrEmpty (ExpectedRepo)) {
				var issueMatch = GithubIssue.Match (line);
				if (issueMatch.Success) {
					string repro = issueMatch.Groups[2].Value;
					if (repro != ExpectedRepo) {
						Explain.Print ($"{line} appears to be cross repro issue '{repro}' vs expected '{ExpectedRepo}' - marking low.");
						return ValidationResults.Low;
					}
				}
			}

			return ValidationResults.Acceptable;
		}

		protected override bool ValidateBugNumber (int bugNumber)
		{
			return bugNumber < 100000;
		}
	}
}
