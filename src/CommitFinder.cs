using System.Collections.Generic;
using System.Linq;
using clio.Model;
using LibGit2Sharp;
using Optional;

namespace clio
{
	public static class CommitFinder
	{
		public static IEnumerable<CommitInfo> Parse (string path, string branchName)
		{
			return Parse (path, branchName, Option.None<string> (), Option.None <string> ());
		}

		public static IEnumerable<CommitInfo> Parse (string path, string branchName, Option<string> startHash, Option<string> endHash)
		{
			try 
			{
				using (var repo = new Repository (path)) 
				{
					var branch = repo.Branches[branchName];
					if (branch == null)
						return Enumerable.Empty<CommitInfo> ();

					return branch.Commits.Select (x => new CommitInfo (x.Sha, x.MessageShort, x.Message)).ToList ();
				}
			}
			catch (RepositoryNotFoundException)
			{
				return Enumerable.Empty<CommitInfo> ();
			}
		}

		public static Option<CommitInfo> ParseSingle (string path, string hash)
		{

			try
			{
				using (var repo = new Repository (path))
				{
					var commit = repo.Lookup<Commit> (hash);
					if (commit == null)
						return Option.None<CommitInfo> ();
					return (new CommitInfo (commit.Sha, commit.MessageShort, commit.Message)).Some ();
				}
			}
			catch (RepositoryNotFoundException)
			{
				return Option.None<CommitInfo> ();
			}
		}
	}
}
