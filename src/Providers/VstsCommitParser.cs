using System;
using System.Text.RegularExpressions;

namespace clio
{
    public sealed class VstsCommitParser : BaseCommitParser
    {
        // TODO: these are not case insensitive, they should be...
        static Regex FullVsts = new Regex(@"htt.*?:\/\/devdiv\.visualstudio\.com\/DevDiv\/_workitems\/edit\/(\d*)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        static Regex Fixes = new Regex(@"fix(es)?[:]*\s*(#)?(\d*)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        static Regex Vsts = new Regex(@"vsts[:]*\s*(#)?(\d*)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        static Regex[] AllRegex = { FullVsts, Fixes };

        public static readonly BaseCommitParser Instance = new VstsCommitParser(AllRegex);

        private VstsCommitParser(Regex[] bugRegexes) : base(IssueSource.Vsts, bugRegexes)
        {
        }

        protected override bool ValidateBugNumber(int bugNumber)
        {
            return bugNumber > 250000;
        }
    }
}
