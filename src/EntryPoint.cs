using System;
using System.Collections.Generic;
using System.Globalization;
using Mono.Options;
using Optional;
using Optional.Unsafe;

namespace clio
{
	class EntryPoint
	{
		public static void Main (string[] args)
		{
			string path = null;
			SearchOptions options = new SearchOptions ();
			SearchRange range = new SearchRange ();
			ActionType requestedAction = ActionType.ListBugs;
			string outputFile = null;

			OptionSet os = new OptionSet ()
			{
				{ "h|?|help", "Displays the help", v => requestedAction = ActionType.Help },
				{ "l|list-commits", "List commits that would be considered", v => requestedAction = ActionType.ListConsideredCommits },
				{ "b|list-bugs", "List bugs discovered instead of formatting release notes", v => requestedAction = ActionType.ListBugs },
				{ "x|export-bugs", "Export bugs discovered instead of formatting release notes", v => requestedAction = ActionType.ExportBugs },
				{ "output-file=", "Output file for export-bugs", v => outputFile = v },
				{ "commit=", "Single commit to consider, sets --oldest and --newest to the same value", s => {
						range.Newest = range.Oldest = s.Some ();
						range.IncludeOldest = true;
					}},
				{ "oldest=", "Starting hash to consider", s => range.Oldest = s.Some () },
				{ "newest=", "Ending hash to consider", e => range.Newest = e.Some () },
				{ "oldest-branch=", "Starting branch to consider. Finds the last commit in master before branch, and ignore all bugs fixed in master that are also fixed in this branch.", s => range.OldestBranch = s.Some () },
				{ "exclude-oldest", "Exclude oldest item from range considered (included by default)", v => range.IncludeOldest = false },
				{ "explain", "Explain why each commit is considered a bug", v => Explain.Enabled = true },
				{ "bugzilla=", "Determines how Bugzilla issues should be validated (public, private, disable). The default is `public`", v =>
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
				{ "vsts=", "Determines if VSTS issues should be validated or not (enable, disable). The default is `disable`", v =>
					{
						switch (v.ToLower (CultureInfo.InvariantCulture))
						{
							case "enable":
								options.Vsts = VstsLevel.Enable;
								break;
							case "disable":
								options.Vsts = VstsLevel.Disable;
								break;
							default:
								Die ($"Unknown value for --vsts {v}");
							break;
						}
					}},
				{ "github-pat=", "Sets the PAT required to access github issues", v => options.GithubPAT = v},
				{ "vsts-pat=", "Sets the PAT required to access VSTS issues", v => options.VstsPAT = v},
				{ "github=", "Project to search issues of, such as xamarin/xamarin-macios. Must be '/' seperated", v => {
						options.IgnoreGithub = false;
						options.GithubLocation = v;
					}},
				{ "ignore-bugzilla", "Ignores Bugilla issues and does not attempt to parse commits for Bugzilla issues", v => options.IgnoreBugzilla = true},
				{ "ignore-vsts", "Ignores VSTS issues and does not attempt to parse commits for VSTS issues", v => options.IgnoreVsts = true},
				{ "additional-bug-info", "Print additional information on each bug for list-bugs", v => options.AdditionalBugInfo = true},
				{ "split-enhancement=", "Split out enhancement bugs from others in listing (defaults to true)", (bool v) => options.SplitEnhancementBugs = v},
				{ "validate-bug-status", "Validate bugzilla status for referenced bugs and report discrepancies (Not closed, not matching milestone)", v => options.ValidateBugStatus = true},
				{ "expected-target-milestone=", "Target Milestone to expect when validate-bug-status (instead of using the most common).", v => options.ExpectedTargetMilestone = v},
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

			if (!options.IgnoreGithub && !options.GithubLocation.Contains ("/"))
					Die ("--github formatted incorrectly");

			if (options.ValidateBugStatus && options.Bugzilla == BugzillaLevel.Disable)
				Die ("Unable to Validate Bug Status with Bugzilla support disabled.");

			if (requestedAction == ActionType.ExportBugs && string.IsNullOrEmpty(outputFile))
				Die ("You must specify --outputfile= with --export-bugs.");

			if (requestedAction != ActionType.ExportBugs && !string.IsNullOrEmpty (outputFile))
				Die ("--outputfile= can only be specified with --export-bugs.");

			if (range.Oldest.HasValue && range.Newest.HasValue)
			{
				if (!CommitFinder.ValidateGitHashes (path, range.Oldest.ValueOrFailure (), range.Newest.ValueOrFailure ()))
					Environment.Exit (-1);
			}

			var request = new clio (path, range, options);
			request.Run (requestedAction, outputFile).Wait ();
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
