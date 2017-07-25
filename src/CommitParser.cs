using System.Collections.Generic;
using System.Linq;
using clio.Model;
using Optional;
using Optional.Linq;
using Optional.Unsafe;
using System;
using System.Text.RegularExpressions;

namespace clio
{
	public static class CommitParser
	{
		static Regex FullBuzilla = new Regex (@"htt.*?://bugzilla\.xamarin\.com[^=]+(=)(\d*)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
		static Regex Buzilla = new Regex (@"bugzilla\s*(\d*)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
		static Regex Bug = new Regex (@"bug\s*(\d*)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
		static Regex Fixes = new Regex (@"fix\s*(\d*)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
		static Regex Short = new Regex (@"bxc\s*(\d*)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

		static Regex[] AllRegex = { FullBuzilla, Buzilla, Bug, Fixes, Short };

		static BugzillaChecker bugzillaChecker;

		static string GetTitle (int id)
		{
			if (bugzillaChecker == null)
			{
				bugzillaChecker = new BugzillaChecker ();
				bugzillaChecker.Setup ().Wait ();
			}

			return bugzillaChecker.GetTitle (id).Result;
		}

		struct ParseResults
		{
			public ParsingConfidence Confidence;
			public string Link;
			public int ID;
			public string BugzillaSummary;
		}

		static ParseResults ParseLine (string line, SearchOptions options)
		{
			foreach (Regex regex in AllRegex)
			{
				var match = regex.Match (line);
				if (match.Success)
				{
					int id;
					if (int.TryParse (match.Groups[match.Groups.Count - 1].Value, out id))
					{
						if (options.Explain)
							Console.WriteLine ($"\tLine \"{line}\" matched pattern {regex}.");

						if (options.IgnoreLowBugs && id < 1000)
							return new ParseResults { Confidence = ParsingConfidence.Invalid };

						if (options.Explain)
							Console.WriteLine ($"\tHad a valid id {id}.");

						ParsingConfidence confidence = ParsingConfidence.High;

						if (line.Contains ("Context") || line.Contains ("context"))
							confidence = ParsingConfidence.Low;

						if (options.Explain)
							Console.WriteLine ($"\tDefault Confidence was {confidence}.");

						string bugzillaSummary = GetTitle (id);
						if (bugzillaSummary == null)
						{
							confidence = ParsingConfidence.Low;
							bugzillaSummary = "";
							if (options.Explain)
								Console.WriteLine ($"\tGiven low confidence due to lack of a matching bugzilla bug.");
						}

						return new ParseResults() { Confidence = confidence, Link = match.Value, ID = id, BugzillaSummary = bugzillaSummary };
					}
				}
			}
			return new ParseResults { Confidence = ParsingConfidence.Invalid };
		}

		public static Option<ParsedCommit> ParseSingle (CommitInfo commit, SearchOptions options)
		{
			if (options.Explain)
				Console.WriteLine ($"Analyzing {commit.Hash}.");

			var textToSearch = commit.Description.SplitLines ();
			var topMatch = textToSearch.Select (x => ParseLine (x, options)).OrderBy (x => x.Confidence).FirstOrDefault ();

			if (topMatch.Confidence != ParsingConfidence.Invalid)
				return new ParsedCommit (commit, topMatch.Link, topMatch.ID, topMatch.Confidence, topMatch.BugzillaSummary).Some ();
			else
				return Option.None<ParsedCommit> ();
		}

		public static IEnumerable<ParsedCommit> Parse (IEnumerable<CommitInfo> commits, SearchOptions options)
		{
			foreach (var commit in commits)
			{
				var parsed = ParseSingle (commit, options);
				if (parsed.HasValue)
					yield return parsed.ValueOrFailure ();
			}
		}
	}
}
 