using System;
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
			var filter = new CommitFilter ();

			options.Oldest.MatchSome (v => filter.ExcludeReachableFrom = v + (options.IncludeOldest ? "~" : ""));
			options.Newest.MatchSome (v => filter.IncludeReachableFrom = v);

			return ParseWithFilter (path, filter);
		}

		public static IEnumerable<CommitInfo> ParseSpecificRange (string path, string oldest, string newest)
		{
			var filter = new CommitFilter { ExcludeReachableFrom = oldest, IncludeReachableFrom = newest };
			return ParseWithFilter (path, filter);
		}

		public static IEnumerable<CommitInfo> ParseWithFilter (string path, CommitFilter filter)
		{
			try
			{
				using (var repo = new Repository (path))
					return repo.Commits.QueryBy (filter).Select (x => new CommitInfo (x.Sha, x.MessageShort, x.Message)).ToList ();
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

		public static Option<Commit> FindMergeBase (string path, string branchName)
		{
			using (var repo = new Repository (path))
			{
				var aCommit = repo.Lookup<Commit> ($"origin/{branchName}");
				var bCommit = repo.Lookup<Commit> ("origin/master");
				if (aCommit == null || bCommit == null)
					return Option.None<Commit>();

				var baseCommit = repo.ObjectDatabase.FindMergeBase (aCommit, bCommit);
				return baseCommit.SomeNotNull ();
			}
		}

		public static bool ValidateGitHashes (string path, string oldest, string newest)
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

				return true;
			}
		}
	}
}
