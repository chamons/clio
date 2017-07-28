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

		static string GetTitle (int id, SearchOptions options)
		{
			if (bugzillaChecker == null)
			{
				bugzillaChecker = new BugzillaChecker (options);
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

		static string StripNewLine (string line) => Regex.Replace (line, @"\r\n?|\n", "");


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
							Console.WriteLine ($"\t\tLine \"{StripNewLine (line)}\" matched pattern {regex}.");

						if (options.IgnoreLowBugs && id < 1000)
							return new ParseResults { Confidence = ParsingConfidence.Invalid };

						if (options.Explain)
							Console.WriteLine ($"\t\tHad a valid id {id}.");

						ParsingConfidence confidence = ParsingConfidence.High;

						if (line.StartsWith ("Context", StringComparison.InvariantCultureIgnoreCase))
							confidence = ParsingConfidence.Invalid;

						if (options.Explain)
							Console.WriteLine ($"\t\tDefault Confidence was {confidence}.");

						string bugzillaSummary = "";
						if (options.Bugzilla != BugzillaLevel.Disable)
						{
							bugzillaSummary = GetTitle (id, options);
							if (bugzillaSummary == null)
							{
								confidence = ParsingConfidence.Low;
								bugzillaSummary = "";
								if (options.Explain)
									Console.WriteLine ($"\t\tGiven low confidence due to lack of a matching bugzilla bug.");
							}
						}

						return new ParseResults() { Confidence = confidence, Link = match.Value, ID = id, BugzillaSummary = bugzillaSummary };
					}
				}
			}
			return new ParseResults { Confidence = ParsingConfidence.Invalid };
		}

		public static IEnumerable<ParsedCommit> ParseSingle (CommitInfo commit, SearchOptions options)
		{
			if (options.Explain)
				Console.WriteLine ($"\tAnalyzing {commit.Hash}.");

			var textToSearch = commit.Description.SplitLines ();

			foreach (var match in textToSearch.Select (x => ParseLine (x, options)).Where (x => x.Confidence != ParsingConfidence.Invalid))
				yield return new ParsedCommit (commit, match.Link, match.ID, match.Confidence, match.BugzillaSummary);
		}

		public static IEnumerable<ParsedCommit> Parse (IEnumerable<CommitInfo> commits, SearchOptions options)
		{
			foreach (var commit in commits)
			{
				foreach (var parsedCommit in ParseSingle (commit, options))
					yield return parsedCommit;
			}
		}


	}
}
 