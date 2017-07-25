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
		public string Output { get; private set; }
		public SearchOptions Options { get; private set; }

		public Option<string> StartHash { get; private set; }
		public Option<string> EndingHash { get; private set; }

		public clio (string path, string output, SearchOptions options)
		{
			Path = path;
			Output = output;
			Options = options;
		}

		public void Run ()
		{
			IEnumerable<CommitInfo> commits = null;

			Options.SingleCommit.Match (single => {
				commits = CommitFinder.ParseSingle (Path, single).Match (x => x.Yield (), () => Enumerable.Empty<CommitInfo> ());
			}, () => { 
				commits = CommitFinder.Parse (Path, Options);
			});

			if (Options.Explain)
				Console.WriteLine ($"Found {commits.Count ()} commits.");

			if (Options.OnlyListCommitsConsidered)
			{
				foreach (var commit in commits)
					Console.WriteLine ($"{commit.Hash} {commit.Title}");

				if (Options.Explain)
					Console.WriteLine ($"Only listing of commits was requested. Exiting.");

				return;
			}

			var parsedCommits = CommitParser.Parse (commits, Options);

			var bugCollection = BugCollector.ClassifyCommits (parsedCommits);

			Console.WriteLine ("Confirmed Bugs:");
			foreach (var bug in bugCollection.ConfirmedBugs)
				Console.WriteLine ($"[{bug.ID}] - {bug.Title}" + (String.IsNullOrEmpty (bug.SecondaryTitle) ? "" : $" / {bug.SecondaryTitle}") );

			Console.WriteLine ("\nUncertain Bugs:");
			foreach (var bug in bugCollection.UncertainBugs)
				Console.WriteLine ($"[{bug.ID}] - {bug.SecondaryTitle}");
		}
	}
}
