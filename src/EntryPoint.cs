using System;
using System.Collections.Generic;
using Mono.Options;
using Optional;
using Optional.Unsafe;

namespace clio
{
	public enum ActionType
	{
		Help,
		ListConsideredCommits,
		ListBugs,
		GenerateReleaseNotes
	}

	class EntryPoint
	{
		public static void Main (string[] args)
		{
			string path = null;
			SearchOptions options = new SearchOptions ();

			ActionType requestedAction = ActionType.Help;

			OptionSet os = new OptionSet ()
			{
				{ "h|?|help", "Displays the help", v => requestedAction = ActionType.Help },
				{ "l|list-commits", "List commits that would be considered", v => requestedAction = ActionType.ListConsideredCommits },
				{ "b|list-bugs", "List bugs discovered instead of formatting release notes", v => requestedAction = ActionType.ListBugs },
				{ "f|format-notes=", $"Output formatted release notes of a given type     ({TemplateGenerator.GetTemplateList ()})", v => {
						if (!TemplateGenerator.ValidateTemplateName (v))
							Die ($"Unable to find template {v} embedded as a resource.");

						options.Template = v.Some ();
						requestedAction = ActionType.GenerateReleaseNotes;
					}},

				{ "o|output=", "Path to output release notes (Defaults to current directory)", o => options.OutputPath = o },
				{ "oldest=", "Starting to consider", s => options.Oldest = s.Some () },
				{ "newest=", "Ending to consider", e => options.Newest = e.Some () },

				{ "single=", "Analyze just a single commit", e => options.SingleCommit = e.Some () },
				{ "exclude-oldest", "Exclude oldest item from range considered (included by default)", v => options.IncludeOldest = false },
				{ "ignore-low-bugs=", "Ignore any bug references to bugs with IDs less than 1000 (Defaults to true)", (bool v) => options.IgnoreLowBugs = v },
				{ "explain", "Explain why each commit is considered a bug", v => options.Explain = true },
				{ "disable-private-buzilla", "Fully logging into bugzilla. Will only validate public bugs.", v => options.DisableBugzillaLogOn = true },
				{ "disable-bugzilla", "Fully disable bugzilla validation of bugs. May increase false positive bugs but drastically reduce time taken.", v => options.DisableBugzillaValidation = true },
				{ "sort-bug-list=", "Sort bug list by id number (Defaults to true)", (bool v) => options.SortBugs = v },
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

			if (options.Oldest.HasValue && options.Newest.HasValue)
			{
				if (!CommitFinder.ValidateGitHashes (path, options.Oldest.ValueOrFailure (), options.Newest.ValueOrFailure ()))
					Environment.Exit (-1);
			}

			var request = new clio (path, options);
			request.Run (requestedAction);	
		}

		static void Die (string v)
		{
			Console.Error.WriteLine (v);
			Environment.Exit (-1);
		}

		static void ShowHelp (OptionSet os)
		{
			Console.WriteLine ("clio [options] path");
			Console.WriteLine ("One action (--list-commits, --list-bugs, --format-notes) must be selected.\n");
			os.WriteOptionDescriptions (Console.Out);
		}
	}
}
