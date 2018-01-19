﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using clio.Model;
using Optional;
using Optional.Unsafe;
using System.Threading.Tasks;

namespace clio
{
	public enum ActionType
	{
		Help,
		ListConsideredCommits,
		ListBugs,
		ExportBugs
	}

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

		public async Task Run (ActionType action, string outputFile)
		{
			// This can mutate Range so must be done first.
			var commitsToIgnore = await ProcessOldestBranchAsync ().ConfigureAwait (false);

			await ProcessAsync (Path, Options, Range, action, commitsToIgnore, outputFile).ConfigureAwait (false);

			if (Options.Submodules)
			{
				bool excludeOldestInSubmodules = false;

				string oldest = Range.Oldest.ValueOr ("");
				string newest = Range.Newest.ValueOr ("HEAD");

				// look one commit back for single commit ranges when examining submodules
				if (oldest == newest)
				{
					excludeOldestInSubmodules = true;
					var oldestParent = CommitFinder.FindFirstParent (Path, oldest);
					if (oldestParent.HasValue)
					{
						oldest = oldestParent.ValueOr(string.Empty);
					}
				}

				var initialSubmoduleStatus = CommitFinder.FindSubmodulesStatus (Path, oldest);

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

					// TOOD: I think we should set IncludeOldest to false here otherwise we will always report the first commit 
					// again when we probably don't want that
					SearchRange submoduleRange = new SearchRange () { 
						IncludeOldest = !excludeOldestInSubmodules,
						Oldest = initialHash.Some (), 
						Newest = finalHash.Some () 
					};

					Explain.Indent ();
					await ProcessAsync (System.IO.Path.Combine (Path, submodule), Options, submoduleRange, action, Enumerable.Empty<ParsedCommit> (), outputFile).ConfigureAwait (false);
					Explain.Deindent ();
				}

				Explain.Deindent ();
			}
		}

		async Task<IEnumerable<ParsedCommit>> ProcessOldestBranchAsync ()
		{
			if (Range.OldestBranch.HasValue)
			{
				var branchName = Range.OldestBranch.ValueOrFailure ();
				if (branchName.StartsWith ("origin/", StringComparison.InvariantCulture))
					branchName = branchName.Remove (0, 7);

				var commitInfo = CommitFinder.FindCommitsOnBranchToIgnore (Path, branchName, Options);

				IEnumerable<ParsedCommit> commitsToIgnore = await CommitParser.ParseAndValidateAsync (commitInfo.Item1, Options).ConfigureAwait (false);

				Explain.Print ($"Found {commitsToIgnore.Count ()} bugs on {branchName} after branch to ignore.");

				Range.Oldest = commitInfo.Item2.Some ();
				Range.IncludeOldest = false;
				return commitsToIgnore;
			}
			return Enumerable.Empty<ParsedCommit> ();
		}

		static async Task ProcessAsync (string path, SearchOptions options, SearchRange range, ActionType action, IEnumerable<ParsedCommit> commitsToIgnore, string outputFile)
		{
			IEnumerable<CommitInfo> commits = CommitFinder.Parse (path, range);

			Explain.Print ($"Found {commits.Count ()} commits.");

			switch (action)
			{
				case ActionType.ListConsideredCommits:
				{
					ConsolePrinter.PrintCommits (commits);
					Explain.Print ($"Only listing of commits was requested. Exiting.");
					return;
				}
				case ActionType.ListBugs:
					await ListBugsAsync (commits, options, commitsToIgnore).ConfigureAwait (false);
					return;
				case ActionType.ExportBugs:
					await ExportAsync (commits, options, commitsToIgnore, outputFile).ConfigureAwait (false);
					return;
				default:
					throw new InvalidOperationException ($"Internal Error - Unknown action requested {action}");
			}
		}

		static async Task ListBugsAsync (IEnumerable<CommitInfo> commits, SearchOptions options, IEnumerable<ParsedCommit> commitsToIgnore)
		{
			var parsedCommits = await CommitParser.ParseAndValidateAsync (commits, options).ConfigureAwait (false);
			var bugCollection = BugCollector.ClassifyCommits (parsedCommits, options, commitsToIgnore);

			ConsolePrinter.PrintBugs (bugCollection, options);

			if (options.ValidateBugStatus)
				BugValidator.Validate (bugCollection, options);
		}

		static async Task ExportAsync(IEnumerable<CommitInfo> commits, SearchOptions options, IEnumerable<ParsedCommit> commitsToIgnore, string outputFile)
		{
			var parsedCommits = await CommitParser.ParseAndValidateAsync (commits, options).ConfigureAwait (false);

			XmlPrinter.ExportBugsToXML (parsedCommits, options, outputFile);
		}
	}
}
