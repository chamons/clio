using System;
using System.Collections.Generic;
using System.Linq;
using clio.Model;
using clio.Providers;

namespace clio
{
	static class ConsolePrinter
	{
		public static void PrintCommits (IEnumerable<CommitInfo> commits)
		{
			foreach (var commit in commits)
				Console.WriteLine ($"{commit.Hash} {commit.Title}");
		}

		public static void PrintBugs (BugCollection bugCollection, SearchOptions options)
		{
			if (options.SplitEnhancementBugs)
			{
				var bugs = bugCollection.Bugs.Where (x => !x.IssueInfo.IsEnhancement);
				PrintBugList ("Bugs:", false, bugs, options);

				var potentialBugs = bugCollection.PotentialBugs.Where (x => !x.IssueInfo.IsEnhancement);
				PrintBugList ("Potential Bugs:", true, potentialBugs, options);

				var enhancements = bugCollection.Bugs.Where (x => x.IssueInfo.IsEnhancement);
				PrintBugList ("Enhancements:", false, enhancements, options);

				var potentialEnhancements = bugCollection.PotentialBugs.Where (x => x.IssueInfo.IsEnhancement);
				PrintBugList ("Potential Enhancements:", true, potentialEnhancements, options);
			}
			else
			{
				PrintBugList ("Bugs:", false, bugCollection.Bugs, options);
				PrintBugList ("Potential Bugs:", true, bugCollection.PotentialBugs, options);
			}
		}

		static void PrintBugList (string title, bool potential, IEnumerable<BugEntry> list, SearchOptions options)
		{
			if (list.Count () > 0)
			{
				Console.WriteLine (title);
				foreach (var bug in list)
					PrintBug (bug, potential, options);
			}
		}

		static string FormatBug (BugEntry bug)
		{
			// If bugzilla validation is disabled, all bugs are uncertain
			if (string.IsNullOrEmpty (bug.Title))
				return FormatUncertainBug (bug);

			return $"* {bug.IssueInfo.IssueSource} [{bug.Id}]({bug.IssueInfo.IssueUrl}) -  {bug.Title}" + (String.IsNullOrEmpty (bug.SecondaryTitle) ? "" : $" / {bug.SecondaryTitle}");
		}

		static string FormatUncertainBug (BugEntry bug)
		{
			return $"* {bug.IssueInfo.IssueSource} [{bug.Id}]({bug.IssueInfo.IssueUrl}) -  {bug.SecondaryTitle}";
		}

		static void PrintBug (BugEntry bug, bool potential, SearchOptions options)
		{
			if (!potential)
				Console.WriteLine (FormatBug (bug));
			else
				Console.WriteLine (FormatUncertainBug (bug));

			if (options.AdditionalBugInfo)
			{
				string additionalInfo = bug.IssueInfo.MoreInfo;
				if (additionalInfo != null)
					Console.WriteLine ($"\t{additionalInfo}");
			}
		}
	}
}
