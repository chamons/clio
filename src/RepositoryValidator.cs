using System;
using System.Linq;
using LibGit2Sharp;

namespace clio
{
	public static class RepositoryValidator
	{
		public static bool ValidateGitHashes (string path, ISearchRange range)
		{
			if (range is HashSearchRange hashSearchRange)
				return HandleHashRange (path, hashSearchRange.Oldest, hashSearchRange.Newest);
			else if (range is BranchSearchRange branchSearchRange)
				return HandleBranchRange (path, branchSearchRange.Base, branchSearchRange.Branch);
			else
				throw new NotImplementedException ();
		}

		static bool HandleBranchRange (string path, string baseBranch, string branch)
		{
			// This will need tweaks under new system
			return true;
		}

		static bool HandleHashRange (string path, string oldest, string newest)
		{
			using (var repo = new Repository (path))
			{
				Commit oldestCommit = repo.Lookup<Commit> (oldest);
				if (oldestCommit == null)
				{
					Console.Error.WriteLine ($"Unable to find starting hash {oldest} in repo.");
					return false;
				}

				Commit newestCommit = repo.Lookup<Commit> (newest);
				if (newestCommit == null)
				{
					Console.Error.WriteLine ($"Unable to find ending hash {newest} in repo.");
					return false;
				}

				if (oldest == newest)
				{
					var commit = repo.Lookup<Commit> (oldest);
					if (commit == null)
					{
						Console.Error.WriteLine ($"Unable to find any commit in range {oldest} to {newest}. Is the order reversed?");
						return false;
					}
				}
				else
				{
					var filter = new CommitFilter
					{
						ExcludeReachableFrom = oldest,
						IncludeReachableFrom = newest
					};

					if (!repo.Commits.QueryBy (filter).Any ())
					{
						Console.Error.WriteLine ($"Unable to find any commit in range {oldest} to {newest}. Is the order reversed?");
						return false;
					}
				}

				return true;
			}
		}
	}
}
