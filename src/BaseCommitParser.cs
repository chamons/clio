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
        static Regex Bug = new Regex(@"bug(:)?\s*(#)?\s*(\d*)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        static Regex Fixes = new Regex(@"fix(es)?(:)?\s*(#)?(\d*)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        protected static Regex[] DefaultLikelyBugRegexes = { Bug, Fixes };

        protected BaseCommitParser(IssueSource issueSource, Regex[] highBugRegexes, Regex[] likelyBugRegexes)
        {
            this.IssueSource = issueSource;
            this.HighBugRegexes = highBugRegexes;
            this.LikelyBugRegexes = likelyBugRegexes;
        }

        protected IssueSource IssueSource { get; private set; }

        /// <summary>
        /// Gets the regexes used to determine bugs with high confidence, for example
        /// commits that reference the full url of the bug
        /// </summary>
        protected Regex[] HighBugRegexes { get; private set; }

        /// <summary>
        /// Gets the regexes used to determine bugs with likely confidence, for example
        /// commits that reference bugs in a general manner 'fixes 12345'
        /// </summary>
        protected Regex[] LikelyBugRegexes { get; private set; }

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

        protected abstract bool ValidateBugNumber(int bugNumber);

        protected virtual bool ShouldIgnoreLine(string line)
        {
            // ignore any lines that look like
            // `Context bug <blah>...`
            // this is most likely setting the background for a bug and not referencing the issue directly
            if (line.StartsWith("Context", StringComparison.InvariantCultureIgnoreCase))
            {
                Explain.Print($"Ignoring because line appears to be setting context for the commit.");
                return true;
            }

            return false;
        }

        ParseResults ProcessLine(Regex regex, string line, ParsingConfidence confidence)
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

                    if (ShouldIgnoreLine(line))
                    {
                        return new ParseResults { Confidence = ParsingConfidence.Invalid };
                    }

                    Explain.Print($"Default Confidence was {confidence}.");
                    return new ParseResults(confidence, match.Value, id);
                }
            }

            return new ParseResults { Confidence = ParsingConfidence.Invalid };
        }

        static string StripNewLine(string line) => Regex.Replace(line, @"\r\n?|\n", "");

        ParseResults ParseLine(string line)
        {
            try
            {
                Explain.Indent();

                // check high quality regexes first, for example, ones that reference the 
                // full url of the bug

                Explain.Print($"Checking regexes for full bug url matches etc");
                foreach (Regex regex in this.HighBugRegexes)
                {
                    var result = ProcessLine(regex, line, ParsingConfidence.High);
                    if (result.Confidence == ParsingConfidence.High) {
                        // we found one with high confidence, lets go
                        return result;
                    }
                }

                // so now, lets look for more generic bug mentions
                Explain.Print($"Checking regexes for generic bug mentions");
                foreach (Regex regex in this.LikelyBugRegexes)
                {
                    var result = ProcessLine(regex, line, ParsingConfidence.Likely);
                    if (result.Confidence == ParsingConfidence.Likely)
                    {
                        // we found one with likely confidence, lets go
                        return result;
                    }
                }

                // nothing found
                return new ParseResults { Confidence = ParsingConfidence.Invalid };
            }
            finally
            {
                Explain.Deindent();
            }
        }

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
    }
}
