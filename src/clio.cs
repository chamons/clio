using System;
using System.Linq;
using Optional;

namespace clio
{
	// Simple top level wrapper interface
	public class clio
	{
		public string Path { get; private set; }
		public string Branch { get; private set; }
		public string Output { get; private set; }
		public SearchOptions Options { get; private set; }

		public Option<string> StartHash { get; private set; }
		public Option<string> EndingHash { get; private set; }

		public clio (string path, string branch, string output, SearchOptions options)
		{
			Path = path;
			Branch = branch;
			Output = output;
			Options = options;
		}

		public void Run ()
		{
			var commits = CommitFinder.Parse (Path, Branch);
			var parsedCommits = CommitParser.Parse (commits, Options);

			foreach (var parsedCommit in parsedCommits)
				Console.WriteLine ($"{parsedCommit.Commit.Hash} ({parsedCommit.Confidence}) - {parsedCommit.BugzillaSummary} / {parsedCommit.Commit.Title} - {parsedCommit.ID} ");
		}
	}
}
