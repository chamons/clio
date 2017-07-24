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
		static Regex FullBuzilla = new Regex (@"htt.*?://bugzilla\.xamarin\.com[^=]+(=)\d*", RegexOptions.Compiled | RegexOptions.IgnoreCase);
		static Regex Buzilla = new Regex (@"bugzilla\s*\d*", RegexOptions.Compiled | RegexOptions.IgnoreCase);
		static Regex Bug = new Regex (@"bug\s*\d*", RegexOptions.Compiled | RegexOptions.IgnoreCase);
		static Regex Fixes = new Regex (@"fix(es)?\s*\d*", RegexOptions.Compiled | RegexOptions.IgnoreCase);
		static Regex Short = new Regex (@"bxc\s*\d*", RegexOptions.Compiled | RegexOptions.IgnoreCase);

		static Regex[] AllRegex = { FullBuzilla, Buzilla, Bug, Fixes, Short };

		static bool BugzillaCheck (string match)
		{
			return true;
		}

		static ValueTuple <ParsingConfidence, string> ParseLine (string line)
		{
			foreach (Regex regex in AllRegex)
			{
				var match = FullBuzilla.Match (line);
				if (match.Success)
				{
					ParsingConfidence confidence = ParsingConfidence.High;

					if (line.Contains ("Context") || line.Contains ("context"))
						confidence = ParsingConfidence.Low;

					if (!BugzillaCheck (match.Value))
						confidence = ParsingConfidence.Low;

					return new ValueTuple<ParsingConfidence, string> (confidence, match.Value);
				}
			}
			return new ValueTuple<ParsingConfidence, string> (ParsingConfidence.Invalid, "");
		}

		public static Option<ParsedCommit> ParseSingle (CommitInfo commit)
		{
			var textToSearch = commit.Title.Yield ().Concat (commit.Description.SplitLines ());
			var topMatch = textToSearch.Select (x => ParseLine (x)).OrderBy (x => x.Item1).FirstOrDefault ();

			if (topMatch.Item1 != ParsingConfidence.Invalid)
				return new ParsedCommit (commit, topMatch.Item2, topMatch.Item1).Some ();
			else
				return Option.None<ParsedCommit> ();
		}

		public static IEnumerable<ParsedCommit> Parse (IEnumerable<CommitInfo> commits)
		{
			foreach (var commit in commits)
			{
				var parsed = ParseSingle (commit);
				if (parsed.HasValue)
					yield return parsed.ValueOrFailure ();
			}
		}
	}
}
 