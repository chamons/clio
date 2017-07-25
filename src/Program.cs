using System;
using System.Collections.Generic;
using Mono.Options;
using Optional;

namespace clio
{
	class EntryPoint
	{
		public static void Main (string[] args)
		{
			clio request = ParseArguments (args);
			request.Run ();
		}

		static clio ParseArguments (string [] args)
		{
			string path = null;
			string output = System.Environment.CurrentDirectory;
			string branch = "master";
			SearchOptions options = new SearchOptions ();

			OptionSet os = null;
			os = new OptionSet ()
			{
				{ "h|?|help", "Displays the help", v => ShowHelp(os) },
				{ "o|output=", "Path to output release notes (Defaults to current directory)", o => output = o },
				{ "s|start=", "Starting hash to consider", s => options.StartingHash = s.Some () },
				{ "e|end=", "Ending hash to consider", e => options.EndingHash = e.Some () },
				{ "b|branch=", "Branch to consider (Defaults to master)", b => branch = b },
				{ "ignore-low-bugs=", "Ignore any bug references to bugs with IDs less than 1000 (Defaults to true)", (bool v) => options.IgnoreLowBugs = v }
			};

			try 
			{
				IList<string> unprocessed = os.Parse (args);
				if (unprocessed.Count != 1)
					ShowHelp (os);
				path = unprocessed [0];
			}
			catch (Exception e) 
			{
				Console.Error.WriteLine ("Could not parse the command line arguments: {0}", e.Message);
			}

			return new clio (path, branch, output, options);
		}

		static void ShowHelp (OptionSet os)
		{
			Console.WriteLine ("clio [options] path");
			os.WriteOptionDescriptions (Console.Out);
			Environment.Exit (1);
		}
	}
}
