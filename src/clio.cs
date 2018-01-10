using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using clio.Model;
using Optional;
using Optional.Unsafe;

namespace clio
{
	// Simple top level wrapper interface
	public class clio
	{
		public string Path { get; private set; }
		public SearchOptions Options { get; private set; }
		public SearchRange Range { get; private set; }

		public clio (string path, SearchRange range, SearchOptions options)
		{
			Path = path;
			Range = range;
			Options = options;
		}

        // TODO: make this async and block on call site
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

				Explain.Print ($"Processing {initialSubmoduleStatus.Count} submodules for change as well");
				Explain.Indent ();

				foreach (var submodule in initialSubmoduleStatus.Keys)
				{
					string initialHash = initialSubmoduleStatus[submodule];
					string finalHash = finalSubmoduleStatus[submodule];

					if (initialHash == finalHash)
					{
						Explain.Print ($"Submodule {submodule} had zero changes ({finalHash}).");
						continue;
					}

					Console.WriteLine ($"\nSubmodule: {submodule}");

					Explain.Print ($"Processing {submodule} submodule from {initialHash} to {finalHash}.");

					SearchRange submoduleRange = new SearchRange () { Oldest = initialHash.Some (), Newest = finalHash.Some () };

					Explain.Indent ();
					Process (System.IO.Path.Combine (Path, submodule), Options, submoduleRange, action, Enumerable.Empty<ParsedCommit> ());
					Explain.Deindent ();
				}

				Explain.Deindent ();
			}
		}

		IEnumerable<ParsedCommit> ProcessOldestBranch ()
		{
			if (Range.OldestBranch.HasValue)
			{
				var branchName = Range.OldestBranch.ValueOrFailure ();
				if (branchName.StartsWith ("origin/", StringComparison.InvariantCulture))
					branchName = branchName.Remove (0, 7);

				var commitInfo = CommitFinder.FindCommitsOnBranchToIgnore (Path, branchName, Options);

                IEnumerable<ParsedCommit> commitsToIgnore = CommitParser.ParseAndValidateAsync (commitInfo.Item1, Options).Result;

				Explain.Print ($"Found {commitsToIgnore.Count ()} bugs on {branchName} after branch to ignore.");

				Range.Oldest = commitInfo.Item2.Some ();
				Range.IncludeOldest = false;
				return commitsToIgnore;
			}
			return Enumerable.Empty<ParsedCommit> ();
		}

		static void Process (string path, SearchOptions options, SearchRange range, ActionType action, IEnumerable<ParsedCommit> commitsToIgnore)
		{
			IEnumerable<CommitInfo> commits = CommitFinder.Parse (path, range);

			Explain.Print ($"Found {commits.Count ()} commits.");

			switch (action)
			{
				case ActionType.ListConsideredCommits:
					PrintCommits (commits);
					return;
				case ActionType.ListBugs:
                    var parsedCommits = CommitParser.ParseAndValidateAsync (commits, options).Result;
					var bugCollection = BugCollector.ClassifyCommits (parsedCommits, options, commitsToIgnore);
					PrintBugs (bugCollection, options);

					if (options.ValidateBugStatus)
						BugValidator.Validate (bugCollection, options);

					return;
				default:
					throw new InvalidOperationException ($"Internal Error - Unknown action requested {action}");
			}
		}

		static void PrintBugs (BugCollection bugCollection, SearchOptions options)
		{
			if (options.SplitEnhancementBugs)
			{
				var bugs = bugCollection.Bugs.Where (x => x.BugInfo.IssueImportance != "enhancement");
				PrintBugList ("Bugs:", false, bugs, options);

				var potentialBugs = bugCollection.PotentialBugs.Where (x => x.BugInfo.IssueImportance != "enhancement");
				PrintBugList ("Potential Bugs:", true, potentialBugs, options);

				var enhancements = bugCollection.Bugs.Where (x => x.BugInfo.IssueImportance == "enhancement");
				PrintBugList ("Enhancements:", false, enhancements, options);

				var potentialEnhancements = bugCollection.PotentialBugs.Where (x => x.BugInfo.IssueImportance == "enhancement");
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

		static void PrintBug (BugEntry bug, bool potential, SearchOptions options)
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

		static void PrintCommits (IEnumerable<CommitInfo> commits)
		{
			foreach (var commit in commits)
				Console.WriteLine ($"{commit.Hash} {commit.Title}");

			Explain.Print ($"Only listing of commits was requested. Exiting.");
		}
	}
}
