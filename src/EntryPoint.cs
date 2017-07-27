using System;
using System.Collections.Generic;
using Mono.Options;
using Optional;

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
				{ "f|format-notes=", "Output formatted release notes of a given type (must be a built in template type)", v => {
						if (!TemplateGenerator.ValidateTemplateName (v))
						{
							Console.Error.WriteLine ($"Unable to find template {v} embedded as a resource.");
							Environment.Exit (-1);
						}

						options.Template = v.Some ();
						requestedAction = ActionType.GenerateReleaseNotes;
					}},

				{ "o|output=", "Path to output release notes (Defaults to current directory)", o => options.OutputPath = o },
				{ "s|starting=", "Starting to consider", s => options.Starting = s.Some () },
				{ "e|ending=", "Ending to consider", e => options.Ending = e.Some () },

				{ "single=", "Analyze just a single commit", e => options.SingleCommit = e.Some () },
				{ "exclude-starting-item", "Exclude starting items from range considered (included by default)", v => options.IncludeStarting = false },
				{ "ignore-low-bugs=", "Ignore any bug references to bugs with IDs less than 1000 (Defaults to true)", (bool v) => options.IgnoreLowBugs = v },
				{ "explain", "Explain why each commit is considered a bug", v => options.Explain = true },
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

			var request = new clio (path, options);
			request.Run (requestedAction);	
		}

		static void ShowHelp (OptionSet os)
		{
			Console.WriteLine ("clio [options] path");
			os.WriteOptionDescriptions (Console.Out);
		}
	}
}
