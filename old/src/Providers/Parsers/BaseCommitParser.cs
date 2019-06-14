using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

using clio.Model;
using Optional.Linq;

namespace clio.Providers.Parsers
{
	public interface ICommitParser
	{
		// Can return more than one if the commit references several issues.
		IEnumerable<ParsedCommit> ParseSingle (CommitInfo commit);
	}

	public abstract class BaseCommitParser : ICommitParser
	{
		static Regex Bug = new Regex (@"bug(:)?\s*(#)?\s*(\d*)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
		static Regex Fixes = new Regex (@"fix(es)?(:)?\s*(#)?(\d*)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

		protected static Regex[] DefaultLikelyBugRegexes = { Bug, Fixes };

		protected IssueSource IssueSource { get; }
		protected Regex[] HighBugRegexes { get; }
		protected Regex[] LikelyBugRegexes { get; }

		protected BaseCommitParser (IssueSource issueSource, Regex[] highBugRegexes, Regex[] likelyBugRegexes)
		{
			IssueSource = issueSource;
			HighBugRegexes = highBugRegexes;
			LikelyBugRegexes = likelyBugRegexes;
		}

		public IEnumerable<ParsedCommit> ParseSingle (CommitInfo commit)
		{
			Explain.Indent ();
			Explain.Print ($"{IssueSource} analyzing {commit.Hash}.");

			var textToSearch = commit.Description.SplitLines ();

			try
			{
				Explain.Indent ();

				Explain.Print ($"Checking regexes for bug mentions");
				foreach (var match in textToSearch.Select (x => ParseLine (x)).Where (x => x.Confidence != ParsingConfidence.Invalid))
					yield return new ParsedCommit (IssueSource, commit, match.Link, match.ID, match.Confidence);
			}
			finally
			{
				Explain.Deindent ();
			}

			Explain.Deindent ();
		}

		protected abstract bool ValidateBugNumber (int bugNumber);
		protected virtual bool ValidateLine (string line, int bugNumber) => true;

		protected virtual bool ShouldIgnoreLine (string line)
		{
			// ignore any lines that look like
			// `Context bug <blah>...`
			// this is most likely setting the background for a bug and not referencing the issue directly
			if (line.StartsWith ("Context", StringComparison.InvariantCultureIgnoreCase))
			{
				Explain.Print ($"Ignoring because line appears to be setting context for the commit.");
				return true;
			}

			return false;
		}

		ParseResults ProcessLine (Regex regex, string line, ParsingConfidence confidence)
		{
			var match = regex.Match (line);
			if (match.Success)
			{
				int id;
				if (int.TryParse (match.Groups[match.Groups.Count - 1].Value, out id))
				{
					Explain.Print ($"Line \"{StripNewLine (line)}\" matched pattern {regex}.");

					if (!ValidateBugNumber (id))
					{
						Explain.Print ($"Had an invalid id {id}.");
						return new ParseResults { Confidence = ParsingConfidence.Invalid };
					}

					if (!ValidateLine (line, id))
						return new ParseResults { Confidence = ParsingConfidence.Invalid };

					Explain.Print ($"Had a valid id {id}.");

					if (ShouldIgnoreLine (line))
						return new ParseResults { Confidence = ParsingConfidence.Invalid };

					Explain.Print ($"Confidence was {confidence}.");
					return new ParseResults (confidence, match.Value, id);
				}
			}

			return new ParseResults { Confidence = ParsingConfidence.Invalid };
		}

		static string StripNewLine (string line) => Regex.Replace (line, @"\r\n?|\n", "");

		ParseResults ParseLine (string line)
		{
			// check high quality regexes first, for example, ones that reference the 
			// full url of the bug

			foreach (Regex regex in this.HighBugRegexes)
			{
				var result = ProcessLine (regex, line, ParsingConfidence.High);
				if (result.Confidence == ParsingConfidence.High)
					return result; // we found one with high confidence, lets go
			}

			// so now, lets look for more generic bug mentions
			foreach (Regex regex in this.LikelyBugRegexes)
			{
				var result = ProcessLine (regex, line, ParsingConfidence.Likely);
				if (result.Confidence == ParsingConfidence.Likely)
					return result; // we found one with likely confidence, lets go
			}

			// nothing found
			return new ParseResults { Confidence = ParsingConfidence.Invalid };
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

			public ParseResults (ParsingConfidence confidence, string link, int id)
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
