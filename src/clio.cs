using System;
using System.Collections.Generic;
using System.Linq;
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
			Process (Path, Options, Range, action);
		}

		static void Process (string path, SearchOptions options, SearchRange range, ActionType action)
		{
			IEnumerable<CommitInfo> commits = null;
			IEnumerable<ParsedCommit> commitsToIgnore = Enumerable.Empty<ParsedCommit> ();

			range.OldestBranch.MatchSome (branchName =>
			{
				if (branchName.StartsWith ("origin/", StringComparison.InvariantCulture))
					branchName = branchName.Remove (0, 7);

				var commitInfo = CommitFinder.FindCommitsOnBranchToIgnore (path, branchName, options);

				commitsToIgnore = CommitParser.Parse (commitInfo.Item1, options);

				if (options.Explain)
					Console.WriteLine ($"Found {commitsToIgnore.Count ()} bugs on {branchName} after branch to ignore.");

				range.Oldest = commitInfo.Item2.Some ();
				range.IncludeOldest = false;
			});

			range.SingleCommit.Match (single =>
			{
				commits = CommitFinder.ParseSingle (path, single).Match (x => x.Yield (), () => Enumerable.Empty<CommitInfo> ());
			}, () =>
			{
				commits = CommitFinder.Parse (path, range);
			});

			if (options.Explain)
				Console.WriteLine ($"Found {commits.Count ()} commits.");

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

			if (action == ActionType.GenerateReleaseNotes)
			{
				TemplateGenerator.GenerateReleaseNotes (bugCollection, options, range);
				return;
			}

			throw new InvalidOperationException ($"Internal Error - Unknown action requested {action}");
		}

		static void PrintBugs (BugCollection bugCollection, SearchOptions options)
		{
			Console.WriteLine ("Bugs:");
			foreach (var bug in bugCollection.Bugs)
				PointBug (bug, false, options);

			if (bugCollection.PotentialBugs.Count () > 0)
			{
				Console.WriteLine ("\tPotential Bugs:");
				foreach (var bug in bugCollection.PotentialBugs)
					PointBug (bug, true, options);
			}
		}

		static void PointBug (BugEntry bug, bool potential, SearchOptions options)
		{
			if (!potential)
				Console.WriteLine (TemplateGenerator.FormatBug (bug, options));
			else
				Console.WriteLine (TemplateGenerator.FormatUncertainBug (bug, options));

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

			if (options.Explain)
				Console.WriteLine ($"Only listing of commits was requested. Exiting.");
		}
	}
}
