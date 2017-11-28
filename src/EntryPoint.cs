using System;
using System.Collections.Generic;
using System.Globalization;
using Mono.Options;
using Optional;
using Optional.Unsafe;

namespace clio
{
	public enum ActionType
	{
		Help,
		ListConsideredCommits,
		ListBugs
	}

	class EntryPoint
	{
		public static void Main (string[] args)
		{
			string path = null;
			SearchOptions options = new SearchOptions ();
			SearchRange range = new SearchRange ();
			ActionType requestedAction = ActionType.ListBugs;

			OptionSet os = new OptionSet ()
			{
				{ "h|?|help", "Displays the help", v => requestedAction = ActionType.Help },
				{ "l|list-commits", "List commits that would be considered", v => requestedAction = ActionType.ListConsideredCommits },
				{ "b|list-bugs", "List bugs discovered instead of formatting release notes", v => requestedAction = ActionType.ListBugs },
				{ "o|output=", "Path to output release notes (Defaults to current directory)", o => options.OutputPath = o },
				{ "oldest=", "Starting hash to consider", s => range.Oldest = s.Some () },
				{ "newest=", "Ending hash to consider", e => range.Newest = e.Some () },
				{ "oldest-branch=", "Starting branch to consider. Finds the last commit in master before branch, and ignore all bugs fixed in master that are also fixed in this branch.", s => range.OldestBranch = s.Some () },

				{ "single=", "Analyze just a single commit", e => range.SingleCommit = e.Some () },
				{ "exclude-oldest", "Exclude oldest item from range considered (included by default)", v => range.IncludeOldest = false },
				{ "ignore-low-bugs=", "Ignore any bug references to bugs with IDs less than 1000 (Defaults to true)", (bool v) => options.IgnoreLowBugs = v },
				{ "explain", "Explain why each commit is considered a bug", v => options.Explain = true },
				{ "bugzilla=", "What level should bugzilla queries be made at      (Public, Private, Disable)", v =>
					{
						switch (v.ToLower (CultureInfo.InvariantCulture))
						{
							case "public":
								options.Bugzilla = BugzillaLevel.Public;
								break;
							case "private":
								options.Bugzilla = BugzillaLevel.Private;
								break;
							case "disable":
								options.Bugzilla = BugzillaLevel.Disable;
								break;
							default:
								Die ($"Unknown value for --bugzilla {v}");
							break;
						}
					}},
				{ "sort-bug-list=", "Sort bug list by id number (Defaults to true)", (bool v) => options.SortBugs = v },
				{ "additional-bug-info", "Print additional information on each bug for list-bugs", v => options.AdditionalBugInfo = true},
				{ "submodules", "Query submodules as well", v => options.Submodules = true},
				new ResponseFileSource (),
			};

			try
			{
				IList<string> unprocessed = os.Parse (args);
				if (requestedAction == ActionType.Help || unprocessed.Count != 1)
				{
					ShowHelp (os);
					return;
				}
				path = unprocessed[0];
			}
			catch (Exception e)
			{
				Console.Error.WriteLine ("Could not parse the command line arguments: {0}", e.Message);
			}

			if (range.Oldest.HasValue && range.Newest.HasValue)
			{
				if (!CommitFinder.ValidateGitHashes (path, range.Oldest.ValueOrFailure (), range.Newest.ValueOrFailure ()))
					Environment.Exit (-1);
			}

			if (options.Submodules && range.SingleCommit.HasValue)
				Die ("Submodules requires a range to consider, not a single entry");

			var request = new clio (path, range, options);
			request.Run (requestedAction);	
		}

		public static void Die (string v)
		{
			Console.Error.WriteLine (v);
			Environment.Exit (-1);
		}

		static void ShowHelp (OptionSet os)
		{
			Console.WriteLine ("clio [options] path");
			Console.WriteLine ("--list-bugs is the default option when none of (--list-commits, --list-bugs) selected.\n");
			os.WriteOptionDescriptions (Console.Out);
		}
	}
}
