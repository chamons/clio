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

		public Option<string> StartHash { get; private set; }
		public Option<string> EndingHash { get; private set; }

		public clio (string path, SearchOptions options)
		{
			Path = path;
			Options = options;
		}

		public void Run (ActionType action)
		{
			IEnumerable<CommitInfo> commits = null;

			Options.SingleCommit.Match (single => {
				commits = CommitFinder.ParseSingle (Path, single).Match (x => x.Yield (), () => Enumerable.Empty<CommitInfo> ());
			}, () => {
				commits = CommitFinder.Parse (Path, Options);
			});

			if (Options.Explain)
				Console.WriteLine ($"Found {commits.Count ()} commits.");

			if (action == ActionType.ListConsideredCommits)
			{
				PrintCommits (commits);
				return;
			}

			var parsedCommits = CommitParser.Parse (commits, Options);
			var bugCollection = BugCollector.ClassifyCommits (parsedCommits, Options);

			if (action == ActionType.ListBugs)
			{
				PrintBugs (bugCollection, Options);
				return;
			}

			if (action == ActionType.GenerateReleaseNotes)
			{
				TemplateGenerator.GenerateReleaseNotes (bugCollection, Options);
				return;
			}

			throw new InvalidOperationException ($"Internal Error - Unknown action requested {action}");
		}

		void PrintBugs (BugCollection bugCollection, SearchOptions options)
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

		void PointBug (BugEntry bug, bool potential, SearchOptions options)
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

		void PrintCommits (IEnumerable<CommitInfo> commits)
		{
			foreach (var commit in commits)
				Console.WriteLine ($"{commit.Hash} {commit.Title}");

			if (Options.Explain)
				Console.WriteLine ($"Only listing of commits was requested. Exiting.");
		}
	}
}
