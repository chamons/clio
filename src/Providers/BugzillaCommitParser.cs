using System;
using System.Text.RegularExpressions;

namespace clio
{
    public sealed class BugzillaCommitParser : BaseCommitParser
    {
        static Regex FullBuzilla = new Regex(@"htt.*?://bugzilla\.xamarin\.com[^=]+(=)(\d*)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        static Regex Buzilla = new Regex(@"bugzilla\s*(#)?(\d*)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        static Regex Bug = new Regex(@"bug\s*(#)?(\d*)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        static Regex Fixes = new Regex(@"fix(es)?\s*(#)?(\d*)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        static Regex Short = new Regex(@"bxc\s*(#)?(\d*)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        static Regex[] AllRegex = { FullBuzilla, Buzilla, Bug, Fixes, Short };

        public static readonly BaseCommitParser Instance = new BugzillaCommitParser(AllRegex);

        private BugzillaCommitParser(Regex[] bugRegexes) : base(IssueSource.Bugzilla, bugRegexes)
        {
        }

        //static BugzillaChecker _bugChecker;

        //protected override BugzillaChecker GetBugChecker(SearchOptions options)
        //{
        //    if (_bugChecker == null)
        //    {
        //        _bugChecker = new BugzillaChecker(options);
        //        _bugChecker.Setup().Wait();
        //    }
        //    return _bugChecker;
        //}

        protected override bool ValidateBugNumber(int bugNumber)
        {
            return (bugNumber >= 1000 && bugNumber <= 250000);
        }

        //protected override async Task InternalDetermineConfidence(ParsedCommit commit)
        //{
        //    // TODO: split out confidence into an abstract online bug thing

        //    //var bugzillaSummary = GetTitle(commit.ID, options);
        //    //if (bugzillaSummary == null)
        //    //{
        //    //    confidence = ParsingConfidence.Low;
        //    //    Explain.Print($"Given low confidence due to lack of a matching bugzilla bug.");
        //    //    return new ParseResults(confidence, match.Value, id);
        //    //}
        //    //var status = GetStatus(id, options);
        //    //var milestone = GetMilestone(id, options);
        //    //var importance = GetImportance(id, options);

        //    //return new ParseResults(confidence, match.Value, id)
        //    //{
        //    //    Summary = bugzillaSummary,
        //    //    Status = status,
        //    //    TargetMilestone = milestone,
        //    //    Importance = importance
        //    //};
        //}
    }
}
