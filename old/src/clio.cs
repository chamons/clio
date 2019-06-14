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
		ExportBugs,
		ExplainCommit
	}

	// Simple top level wrapper interface
	public class clio
	{
		public string Path { get; private set; }
		public SearchOptions Options { get; private set; }

		public clio (string path, SearchOptions options)
		{
			Path = path;
			Options = options;
		}

		public async Task Run (ActionType action, ISearchRange range, string outputFile) => await ProcessAsync (Path, Options, range, action, outputFile).ConfigureAwait (false);

		static async Task ProcessAsync (string path, SearchOptions options, ISearchRange range, ActionType action, string outputFile)
		{
			IEnumerable<CommitInfo> commits;
			if (range is HashSearchRange hashSearchRange)
				commits = CommitFinder.ParseHashRange (path, options, hashSearchRange.Oldest, hashSearchRange.Newest);
			else if (range is BranchSearchRange branchSearchRange)
				commits = CommitFinder.ParseBranchRange (path, options, branchSearchRange.Base, branchSearchRange.Branch);
			else if (range is SingleHashSearchRange singleRange)
				commits = CommitFinder.ParseSingle (path, options, singleRange.Hash);
			else
				throw new NotImplementedException ();

			Explain.Print ($"Found {commits.Count ()} commits.");

			switch (action)
			{
				case ActionType.ListConsideredCommits:
					ConsolePrinter.Create (options).PrintCommits (commits);
					Explain.Print ($"Only listing of commits was requested. Exiting.");
					return;
				case ActionType.ExplainCommit:
					Explain.Enabled = true;
					await ListBugsAsync (commits, options).ConfigureAwait (false);
					return;
				case ActionType.ListBugs:
					await ListBugsAsync (commits, options).ConfigureAwait (false);
					return;
				case ActionType.ExportBugs:
					await ExportAsync (commits, options, outputFile).ConfigureAwait (false);
					return;
				default:
					throw new InvalidOperationException ($"Internal Error - Unknown action requested {action}");
			}
		}

		static async Task ListBugsAsync (IEnumerable<CommitInfo> commits, SearchOptions options)
		{
			var parsedCommits = await CommitParser.ParseAndValidateAsync (commits, options).ConfigureAwait (false);
			var bugCollection = BugCollector.ClassifyCommits (parsedCommits);

			ConsolePrinter.Create (options).PrintBugs (bugCollection);
		}

		static async Task ExportAsync(IEnumerable<CommitInfo> commits, SearchOptions options, string outputFile)
		{
			var parsedCommits = await CommitParser.ParseAndValidateAsync (commits, options).ConfigureAwait (false);

			XmlPrinter.ExportBugsToXML (parsedCommits, options, outputFile);
		}
	}
}