using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using clio.Model;
using Optional;

namespace clio
{
	// Simple top level wrapper interface
	public class clio
	{
		public string Path { get; private set; }
		public SearchOptions Options { get; private set; }
		public SearchRange Range { get; private set; }

		public Option<string> StartHash { get; private set; }
		public Option<string> EndingHash { get; private set; }

		public clio (string path, SearchRange range, SearchOptions options)
		{
			Path = path;
			Range = range;
			Options = options;
		}

		public void Run (ActionType action)
		{
			// This can mutage Range so must be done first.
			var commitsToIgnore = ProcessOldestBranch ();

			Process (Path, Options, Range, action, commitsToIgnore);

			if (Options.Submodules)
			{
				string oldest = Range.Oldest.ValueOr ("");
				var initialSubmoduleStatus = CommitFinder.FindSubmodulesStatus (Path, oldest);

				string newest = Range.Newest.ValueOr ("HEAD");
				var finalSubmoduleStatus = CommitFinder.FindSubmodulesStatus (Path, newest);

				Options.PrintExplain ($"Processing {initialSubmoduleStatus.Count} submodules for change as well");
				Options.IndentExplain ();

				foreach (var submodule in initialSubmoduleStatus.Keys)
				{
					string initialHash = initialSubmoduleStatus[submodule];
					string finalHash = finalSubmoduleStatus[submodule];

					if (initialHash == finalHash)
					{
						Options.PrintExplain ($"Submodule {submodule} had zero changes ({finalHash}).");
						continue;
					}

					Console.WriteLine ($"\nSubmodule: {submodule}");

					Options.PrintExplain ($"Processing {submodule} submodule from {initialHash} to {finalHash}.");

					SearchRange submoduleRange = new SearchRange () { Oldest = initialHash.Some (), Newest = finalHash.Some () };

					Options.IndentExplain ();
					Process (System.IO.Path.Combine (Path, submodule), Options, submoduleRange, action, Enumerable.Empty<ParsedCommit> ());
					Options.DeindentExplain ();
				}

				Options.DeindentExplain ();
			}
		}

		IEnumerable<ParsedCommit> ProcessOldestBranch ()
		{
			IEnumerable<ParsedCommit> commitsToIgnore = Enumerable.Empty<ParsedCommit> ();

			Range.OldestBranch.MatchSome (branchName =>
			{
				if (branchName.StartsWith ("origin/", StringComparison.InvariantCulture))
					branchName = branchName.Remove (0, 7);

				var commitInfo = CommitFinder.FindCommitsOnBranchToIgnore (Path, branchName, Options);

				commitsToIgnore = CommitParser.Parse (commitInfo.Item1, Options);

				Options.PrintExplain ($"Found {commitsToIgnore.Count ()} bugs on {branchName} after branch to ignore.");

				Range.Oldest = commitInfo.Item2.Some ();
				Range.IncludeOldest = false;
			});
			return commitsToIgnore;
		}

		static void Process (string path, SearchOptions options, SearchRange range, ActionType action, IEnumerable<ParsedCommit> commitsToIgnore)
		{
			IEnumerable<CommitInfo> commits = null;

			range.SingleCommit.Match (single =>
			{
				commits = CommitFinder.ParseSingle (path, single).Match (x => x.Yield (), () => Enumerable.Empty<CommitInfo> ());
			}, () =>
			{
				commits = CommitFinder.Parse (path, range);
			});

			options.PrintExplain ($"Found {commits.Count ()} commits.");

			if (action == ActionType.ListConsideredCommits)
			{
				PrintCommits (commits, options);
				return;
			}

			var parsedCommits = CommitParser.Parse (commits, options).ToList ();
			var bugCollection = BugCollector.ClassifyCommits (parsedCommits, options, commitsToIgnore);

			if (action == ActionType.ListBugs)
			{
				PrintBugs (bugCollection, options);
				return;
			}

			throw new InvalidOperationException ($"Internal Error - Unknown action requested {action}");
		}

		static void PrintBugs (BugCollection bugCollection, SearchOptions options)
		{
			if (bugCollection.Bugs.Count () > 0)
			{
				Console.WriteLine ("Bugs:");
				foreach (var bug in bugCollection.Bugs)
					PointBug (bug, false, options);
			}

			if (bugCollection.PotentialBugs.Count () > 0)
			{
				Console.WriteLine ("Potential Bugs:");
				foreach (var bug in bugCollection.PotentialBugs)
					PointBug (bug, true, options);
			}
		}

		public static string FormatBug (BugEntry bug)
		{
			// If bugzilla validation is disabled, all bugs are uncertain
			if (string.IsNullOrEmpty (bug.Title))
				return FormatUncertainBug (bug);

			return $"* [{bug.ID}](https://bugzilla.xamarin.com/show_bug.cgi?id={bug.ID}) -  {bug.Title}" + (String.IsNullOrEmpty (bug.SecondaryTitle) ? "" : $" / {bug.SecondaryTitle}");
		}

		public static string FormatUncertainBug (BugEntry bug)
		{
			return $"* [{bug.ID}](https://bugzilla.xamarin.com/show_bug.cgi?id={bug.ID}) -  {bug.SecondaryTitle}";
		}

		static void PointBug (BugEntry bug, bool potential, SearchOptions options)
		{
			if (!potential)
				Console.WriteLine (FormatBug (bug));
			else
				Console.WriteLine (FormatUncertainBug (bug));

			if (options.AdditionalBugInfo)
			{
				BugzillaChecker checker = new BugzillaChecker (options);
				checker.Setup ().Wait ();
				string additionalInfo = checker.LookupAdditionalInfo (bug.ID).Result;
				if (additionalInfo != null)
					Console.WriteLine ($"\t{additionalInfo}");
			}
		}

		static void PrintCommits (IEnumerable<CommitInfo> commits, SearchOptions options)
		{
			foreach (var commit in commits)
				Console.WriteLine ($"{commit.Hash} {commit.Title}");

			options.PrintExplain ($"Only listing of commits was requested. Exiting.");
		}
	}
}
