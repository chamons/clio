using System;
using System.Text.RegularExpressions;

namespace clio
{
    public sealed class VstsCommitParser : BaseCommitParser
    {
        static Regex FullVsts = new Regex(@"htt.*?:\/\/devdiv\.visualstudio\.com\/DevDiv\/_workitems\/edit\/(\d*)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        static Regex Vsts = new Regex(@"vsts[:]*\s*(#)?(\d*)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        static Regex[] AllRegex = { FullVsts, Vsts };

        public static readonly BaseCommitParser Instance = new VstsCommitParser(AllRegex);

        private VstsCommitParser(Regex[] bugRegexes) : base(IssueSource.Vsts, bugRegexes, DefaultLikelyBugRegexes)
        {
        }

        protected override bool ValidateBugNumber(int bugNumber)
        {
            return bugNumber > 250000;
        }
    }
}
