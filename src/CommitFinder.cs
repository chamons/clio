using System.Collections.Generic;
using System.Linq;
using clio.Model;
using LibGit2Sharp;
using Optional;

namespace clio
{
	public static class CommitFinder
	{
		public static IEnumerable<CommitInfo> Parse (string path, SearchOptions options)
		{
			try 
			{
				using (var repo = new Repository (path)) 
				{
					var filter = new CommitFilter ();
					options.Starting.MatchSome (v => filter.ExcludeReachableFrom = v + (options.IncludeStarting ? "~" : ""));
					options.Ending.MatchSome (v => filter.IncludeReachableFrom = v);

					return repo.Commits.QueryBy (filter).Select (x => new CommitInfo (x.Sha, x.MessageShort, x.Message)).ToList ();
				}
			}
			catch (RepositoryNotFoundException)
			{
				return Enumerable.Empty<CommitInfo> ();
			}
			catch (NotFoundException)
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
