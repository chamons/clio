using System;
using System.Collections.Generic;
using System.Linq;
using clio.Model;
using clio.Providers;

namespace clio
{
	/// <summary>
	/// Prints information to the console
	/// </summary>
	static class ConsolePrinter
	{
		/// <summary>
		/// Prints the given commits to the console
		/// </summary>
		/// <param name="commits">Commits.</param>
		public static void PrintCommits (IEnumerable<CommitInfo> commits)
		{
			foreach (var commit in commits)
				Console.WriteLine ($"{commit.Hash} {commit.Title}");

			Explain.Print ($"Only listing of commits was requested. Exiting.");
		}

		/// <summary>
		/// Prints the given bug collection to the console
		/// </summary>
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

			return $"* [{bug.Id}]({bug.IssueInfo.IssueUrl}) -  {bug.Title}" + (String.IsNullOrEmpty (bug.SecondaryTitle) ? "" : $" / {bug.SecondaryTitle}");
		}

		static string FormatUncertainBug (BugEntry bug)
		{
			return $"* [{bug.Id}]({bug.IssueInfo.IssueUrl}) -  {bug.SecondaryTitle}";
		}

		static void PrintBug (BugEntry bug, bool potential, SearchOptions options)
		{
			if (!potential)
				Console.WriteLine (FormatBug (bug));
			else
				Console.WriteLine (FormatUncertainBug (bug));

			if (options.AdditionalBugInfo)
			{
				var checker = new BugzillaIssueValidator (options);
				string additionalInfo = checker.GetIssueAsync ((int)bug.Id).Result.MoreInfo;
				if (additionalInfo != null)
					Console.WriteLine ($"\t{additionalInfo}");
			}
		}
	}
}
