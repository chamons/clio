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

		public Option<string> StartHash { get; private set; }
		public Option<string> EndingHash { get; private set; }

		public clio (string path, string branch, string output, Option<string> startHash, Option<string> endingHash)
		{
			Path = path;
			Branch = branch;
			Output = output;
			StartHash = startHash;
			EndingHash = endingHash;
		}

		public void Run ()
		{
			Console.WriteLine ($"Running on {Path} ({Branch}), outputting to {Output}.");

			var commits = CommitFinder.Parse (Path, Branch);
			Console.WriteLine (commits.Count ());
		}
	}
}
