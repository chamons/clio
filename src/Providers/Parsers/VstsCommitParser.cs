using System;
using System.Text.RegularExpressions;
using clio.Model;

namespace clio.Providers.Parsers
{
	public sealed class VstsCommitParser : BaseCommitParser
	{
		static Regex FullVsts = new Regex (@"htt.*?:\/\/devdiv\.visualstudio\.com\/DevDiv\/_workitems\/edit\/(\d*)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
		static Regex Vsts = new Regex (@"vsts[:]*\s*(#)?(\d*)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
		static Regex VstsBug = new Regex (@"vsts\s*bug[:]*\s*(#)?(\d*)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

		static Regex[] AllRegex = { FullVsts, Vsts, VstsBug };

		public static readonly BaseCommitParser Instance = new VstsCommitParser (AllRegex);

		private VstsCommitParser (Regex[] bugRegexes) : base (IssueSource.Vsts, bugRegexes, DefaultLikelyBugRegexes)
		{
		}

		protected override bool ValidateBugNumber (int bugNumber)
		{
			return bugNumber > 250000;
		}
	}
}
