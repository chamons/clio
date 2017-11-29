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
		static Regex Buzilla = new Regex (@"bugzilla\s*(#)?(\d*)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
		static Regex Bug = new Regex (@"bug\s*(#)?(\d*)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
		static Regex Fixes = new Regex (@"fix(es)?\s*(#)?(\d*)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
		static Regex Short = new Regex (@"bxc\s*(#)?(\d*)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

		static Regex[] AllRegex = { FullBuzilla, Buzilla, Bug, Fixes, Short };

		static string GetTitle (int id, SearchOptions options)
		{
			var checker = GetBugChecker (options);
			return checker.LookupTitle (id).Result;
		}

		static string GetStatus (int id, SearchOptions options)
		{
			var checker = GetBugChecker (options);
			return checker.LookupStatus (id).Result;
		}

		static string GetMilestone (int id, SearchOptions options)
		{
			var checker = GetBugChecker (options);
			return checker.LookupTargetMilestone (id).Result;
		}

		static string GetImportance (int id, SearchOptions options)
		{
			var checker = GetBugChecker (options);
			return checker.LookupImportance (id).Result;
		}

		static BugzillaChecker _bugChecker;
		static BugzillaChecker GetBugChecker (SearchOptions options)
		{
			if (_bugChecker == null)
			{
				_bugChecker = new BugzillaChecker (options);
				_bugChecker.Setup ().Wait ();
			}
			return _bugChecker;
		}

		struct ParseResults
		{
			public ParsingConfidence Confidence;
			public string Link;
			public int ID;
			public string BugzillaSummary;
			public string TargetMilestone;
			public string Status;
			public string Importance;

			public ParseResults (ParsingConfidence confidence, string link, int id)
			{
				Confidence = confidence;
				Link = link;
				ID = id;
				BugzillaSummary = "";
				TargetMilestone = "";
				Status = "";
				Importance = "";
			}
		}

		static string StripNewLine (string line) => Regex.Replace (line, @"\r\n?|\n", "");

		static ParseResults ParseLine (string line, SearchOptions options)
		{
			try
			{
				Explain.Indent ();
				foreach (Regex regex in AllRegex)
				{
					var match = regex.Match (line);
					if (match.Success)
					{
						int id;
						if (int.TryParse (match.Groups[match.Groups.Count - 1].Value, out id))
						{
							Explain.Print ($"Line \"{StripNewLine (line)}\" matched pattern {regex}.");

							if (id < 1000 || id > 200000)
							{
								Explain.Print ($"Had an invalid id {id}.");
								return new ParseResults { Confidence = ParsingConfidence.Invalid };
							}

							Explain.Print ($"Had a valid id {id}.");

							ParsingConfidence confidence = ParsingConfidence.High;

							if (line.StartsWith ("Context", StringComparison.InvariantCultureIgnoreCase))
								confidence = ParsingConfidence.Invalid;

							Explain.Print ($"Default Confidence was {confidence}.");
							if (options.Bugzilla != BugzillaLevel.Disable)
							{
								var bugzillaSummary = GetTitle (id, options);
								if (bugzillaSummary == null)
								{
									confidence = ParsingConfidence.Low;
									Explain.Print ($"Given low confidence due to lack of a matching bugzilla bug.");
									return new ParseResults (confidence, match.Value, id);
								}
								var status = GetStatus (id, options);
								var milestone = GetMilestone (id, options);
								var importance = GetImportance (id, options);

								return new ParseResults (confidence, match.Value, id)
								{
									BugzillaSummary = bugzillaSummary, 
									Status = status, 
									TargetMilestone = milestone, 
									Importance = importance 
								};
							}
							return new ParseResults (confidence, match.Value, id);
						}
					}
				}
				return new ParseResults { Confidence = ParsingConfidence.Invalid };
			}
			finally
			{
				Explain.Deindent ();
			}
		}

		public static IEnumerable<ParsedCommit> ParseSingle (CommitInfo commit, SearchOptions options)
		{
			Explain.Indent ();
			Explain.Print ($"Analyzing {commit.Hash}.");

			var textToSearch = commit.Description.SplitLines ();

			foreach (var match in textToSearch.Select (x => ParseLine (x, options)).Where (x => x.Confidence != ParsingConfidence.Invalid))
				yield return new ParsedCommit (commit, match.Link, match.ID, match.Confidence, match.BugzillaSummary, match.TargetMilestone, match.Status, match.Importance);
			Explain.Deindent ();
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
 