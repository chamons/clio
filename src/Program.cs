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
			Option<string> startHash = Optional.Option.None<string> ();
			Option<string> endingHash = Optional.Option.None<string> ();

			OptionSet os = null;
			os = new OptionSet ()
			{
				{ "h|?|help", "Displays the help", v => ShowHelp(os) },
				{ "o|output=", "Path to output release notes (Defaults to current directory)", o => output = o },
				{ "s|start=", "Starting hash to consider", s => startHash = s.Some () },
				{ "e|end=", "Ending hash to consider", e => endingHash = e.Some () },
				{ "b|branch=", "Branch to consider (Defaults to master)", b => branch = b },
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

			return new clio (path, branch, output, startHash, endingHash);
		}

		static void ShowHelp (OptionSet os)
		{
			Console.WriteLine ("clio [options] path");
			os.WriteOptionDescriptions (Console.Out);
			Environment.Exit (1);
		}
	}
}
