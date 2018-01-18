using System;
using System.Text.RegularExpressions;
using clio.Model;

namespace clio.Providers.Parsers
{
	public sealed class BugzillaCommitParser : BaseCommitParser
	{
		static Regex FullBuzilla = new Regex (@"htt.*?://bugzilla\.xamarin\.com[^=]+(=)(\d*)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
		static Regex Buzilla = new Regex (@"bugzilla\s*(#)?(\d*)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
		static Regex ShortBuzilla = new Regex (@"bxc\s*(#)?(\d*)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

		static Regex[] AllRegex = { FullBuzilla, Buzilla, ShortBuzilla };

		public static readonly BaseCommitParser Instance = new BugzillaCommitParser (AllRegex);

		private BugzillaCommitParser (Regex[] bugRegexes) : base (IssueSource.Bugzilla, bugRegexes, DefaultLikelyBugRegexes)
		{
		}

		protected override bool ValidateBugNumber (int bugNumber)
		{
			return (bugNumber >= 1000 && bugNumber <= 250000);
		}
	}
}
