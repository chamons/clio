using System;
using System.Collections.Generic;
using System.Linq;
using clio.Model;
using Optional.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace clio
{
    /// <summary>
    /// Base commit parser implementation
    /// </summary>
    public abstract class BaseCommitParser : ICommitParser
    {
        protected BaseCommitParser(IssueSource issueSource, Regex[] bugRegexes)
        {
            this.IssueSource = issueSource;
            this.BugRegexes = bugRegexes;
        }

        protected IssueSource IssueSource { get; private set; }

        protected Regex[] BugRegexes { get; private set; }

        static string StripNewLine(string line) => Regex.Replace(line, @"\r\n?|\n", "");

        struct ParseResults
        {
            public ParsingConfidence Confidence;
            public string Link;
            public int ID;
            public string Summary;
            public string TargetMilestone;
            public string Status;
            public string Importance;

            public ParseResults(ParsingConfidence confidence, string link, int id)
            {
                Confidence = confidence;
                Link = link;
                ID = id;
                Summary = "";
                TargetMilestone = "";
                Status = "";
                Importance = "";
            }
        }

        //protected abstract BugzillaChecker GetBugChecker(SearchOptions options);

        protected abstract bool ValidateBugNumber(int bugNumber);

        //protected string GetTitle(int id, SearchOptions options)
        //{
        //    var checker = GetBugChecker(options);
        //    return checker.LookupTitle(id).Result;
        //}

        //protected string GetStatus(int id, SearchOptions options)
        //{
        //    var checker = GetBugChecker(options);
        //    return checker.LookupStatus(id).Result;
        //}

        //protected string GetMilestone(int id, SearchOptions options)
        //{
        //    var checker = GetBugChecker(options);
        //    return checker.LookupTargetMilestone(id).Result;
        //}

        //protected string GetImportance(int id, SearchOptions options)
        //{
        //    var checker = GetBugChecker(options);
        //    return checker.LookupImportance(id).Result;
        //}

        ParseResults ParseLine(string line)
        {
            try
            {
                Explain.Indent();
                foreach (Regex regex in this.BugRegexes)
                {
                    var match = regex.Match(line);
                    if (match.Success)
                    {
                        int id;
                        if (int.TryParse(match.Groups[match.Groups.Count - 1].Value, out id))
                        {
                            Explain.Print($"Line \"{StripNewLine(line)}\" matched pattern {regex}.");

                            if (!ValidateBugNumber(id))
                            {
                                Explain.Print($"Had an invalid id {id}.");
                                return new ParseResults { Confidence = ParsingConfidence.Invalid };
                            }

                            Explain.Print($"Had a valid id {id}.");

                            ParsingConfidence confidence = ParsingConfidence.High;

                            // ignore any lines that look like
                            // `Context bug <blah>...`
                            // this is most likely setting the background for a bug and not referencing the issue directly
                            if (line.StartsWith("Context", StringComparison.InvariantCultureIgnoreCase))
                                confidence = ParsingConfidence.Invalid;

                            Explain.Print($"Default Confidence was {confidence}.");
                            return new ParseResults(confidence, match.Value, id);
                        }
                    }
                }
                return new ParseResults { Confidence = ParsingConfidence.Invalid };
            }
            finally
            {
                Explain.Deindent();
            }
        }

        public IEnumerable<ParsedCommit> ParseSingle(CommitInfo commit)
        {
            Explain.Indent();
            Explain.Print($"Analyzing {commit.Hash}.");

            var textToSearch = commit.Description.SplitLines();

            foreach (var match in textToSearch.Select(x => ParseLine(x))
                     .Where(x => x.Confidence != ParsingConfidence.Invalid))
            {
                yield return new ParsedCommit(this.IssueSource, commit, match.Link, match.ID, match.Confidence);
            }

            Explain.Deindent();
        }
    }
}
