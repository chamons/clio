using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;
using System.Linq;

using Mono.Options;

using Clio.Ranges;
using Clio.Requests;
using Clio.Utilities;

namespace Clio
{
   	public enum ActionType
	{
		Help,
		ExplainCommit,
		List,
	}

	class Program
	{
		static async Task Main(string[] args)
		{
			bool collectAuthors = false;	
		   	string path = null;
			SearchOptions options = new SearchOptions ();
			ISearchRange range = null;
			ActionType requestedAction = ActionType.List;

			void SetBranchRange (string baseBranch, string branch)
			{
				if (range != null && !(range is BranchSearchRange))
					Errors.Die ("Can not mix hash and branch ranges");
				BranchSearchRange current = range != null ? (BranchSearchRange)range : new BranchSearchRange ();
				if (baseBranch != null)
					current.Base = baseBranch;
				if (branch != null)
					current.Branch = branch;
				range = current;
			}

			void SetHashRange (string oldest, string newest)
			{
				if (range != null && !(range is HashSearchRange))
					Errors.Die ("Can not mix hash and branch ranges");
				HashSearchRange current = range != null ? (HashSearchRange)range : new HashSearchRange ();
				if (oldest != null)
					current.Oldest = oldest;
				if (newest != null)
					current.Newest = newest;
				range = current;
			}

			OptionSet os = new OptionSet ()
			{
				{ "h|?|help", "Displays the help", v => requestedAction = ActionType.Help },
				{ "verbose", "Print more details", v => Explain.Enabled = true },
				{ "explain-commit=", "Parse a single commit and explain.", v => {
					requestedAction = ActionType.ExplainCommit;
					range = new SingleHashSearchRange { Hash = v };
				}},
				{ "oldest=", "Starting hash to consider (hash range mode)", s => SetHashRange (s, null) },
				{ "newest=", "Ending hash to consider (hash range mode)", e => SetHashRange (null, e) },
				{ "base=", "Starting base to consider (branch/base mode)", b => SetBranchRange (b, null) },
				{ "branch=", "Ending branch to consider (branch/base mode)", b => SetBranchRange (null, b) },

				{ "explain", "Explain why each commit is considered a bug", v => Explain.Enabled = true },
				{ "github-pat=", "Sets the PAT required to access github issues", v => options.GithubPAT = v},
				{ "github-pat-file=", "Sets a file to read to use for github-path", v => options.GithubPAT = File.ReadLines (v).First ()},
				{ "github=", "Project to search issues of, such as xamarin/xamarin-macios. Must be '/' seperated", v => {
						options.GithubLocation = v;
					}},
				{ "collect-authors", "Generate a list of unique authors to commits listed", v => collectAuthors = true},
				{ "ignore=", "Commit hashes to ignore", v => options.CommitsToIgnore.Add (v) },
				new ResponseFileSource (),
			};

			try {
				IList<string> unprocessed = os.Parse (args);
				if (requestedAction == ActionType.Help || unprocessed.Count != 1) {
					ShowHelp (os);
					return;
				}
				path = unprocessed[0];
			}
			catch (Exception e) {
				Console.Error.WriteLine ("Could not parse the command line arguments: {0}", e.Message);
				return;
			}

			if (String.IsNullOrEmpty (options.GithubLocation) || !options.GithubLocation.Contains ("/"))
				Errors.Die ("--github formatted incorrectly");
					
			if (!RepositoryValidator.ValidateGitHashes (path, range))
				Environment.Exit (-1);

			if (String.IsNullOrEmpty (options.GithubPAT))
				Errors.Die ("Unable to read GitHub PAT token");

			Explain.Print ("Finding Commits in Range");
			Explain.Indent ();
			var commits = RangeFinder.Find (path, options, range).ToList ();
			Explain.Print ($"Found: {commits.Count}");
			Explain.Deindent ();

			Explain.Print ("Finding Pull Requests");
			Explain.Indent ();
			var finder = new RequestFinder (options.GithubPAT);
			await finder.AssertLimits ();
			var requestCollection = await finder.FindPullRequests (options.GithubLocation, commits);
			Explain.Print ($"Found: {requestCollection.All.Count}");
			Explain.Deindent ();

			var printer = new ConsolePrinter (requestCollection, options.GithubLocation);
			printer.Print (collectAuthors);
		}

		static void ShowHelp (OptionSet os)
		{
			Console.WriteLine ("clio [options] path");
			os.WriteOptionDescriptions (Console.Out);
		}
	}
}
