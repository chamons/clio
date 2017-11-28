using System;
using System.Collections.Generic;
using System.Linq;
using clio.Model;
using LibGit2Sharp;
using Optional;
using Optional.Unsafe;

namespace clio
{
	public static class CommitFinder
	{
		public static IEnumerable<CommitInfo> Parse (string path, SearchRange range)
		{
			var filter = new CommitFilter ();

			range.Oldest.MatchSome (v => filter.ExcludeReachableFrom = v + (range.IncludeOldest ? "~" : ""));
			range.Newest.MatchSome (v => filter.IncludeReachableFrom = v);

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

		public static Option<string> FindMergeBase (string path, string branchName)
		{
			using (var repo = new Repository (path))
			{
				var aCommit = repo.Lookup<Commit> ($"origin/{branchName}");
				var bCommit = repo.Lookup<Commit> ("origin/master");
				if (aCommit == null || bCommit == null)
					return Option.None<string>();

				var baseCommit = repo.ObjectDatabase.FindMergeBase (aCommit, bCommit);
				return baseCommit.Sha.SomeNotNull ();
			}
		}

		public static ValueTuple<IEnumerable<CommitInfo>, string> FindCommitsOnBranchToIgnore (string path, string branchName, SearchOptions options)
		{
			var merge = FindMergeBase (path, branchName);
			if (!merge.HasValue)
			{
				EntryPoint.Die ($"Unable to find merge-base with {branchName} on {path}. Do you need to get fetch?");
				return new ValueTuple<IEnumerable<CommitInfo>, string> ();
			}

			var mergeBase = merge.ValueOrFailure ();

			Explain.Print ($"Found merge base for {branchName} at {mergeBase}.");

			var commitToIgnoreOnBranch = ParseSpecificRange (path, mergeBase, $"origin/{branchName}");

			Explain.Print ($"Found {commitToIgnoreOnBranch.Count ()} commits on {branchName} after branch.");

			return new ValueTuple<IEnumerable<CommitInfo>, string> (commitToIgnoreOnBranch, mergeBase);
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

		public static Dictionary<string, string> FindSubmodulesStatus (string path, string hash)
		{
			Dictionary<string, string> submoduleStatus = new Dictionary<string, string> ();

			// This assumes submodules did not drastically change over a single release
			using (var repo = new Repository (path))
			{
				var commit = repo.Lookup <Commit> (hash);

				foreach (var submodule in repo.Submodules.Select (x => x.Path))
				{
					var submoduleObject = commit[submodule];
					if (submoduleObject != null)
						submoduleStatus[submodule] = submoduleObject.Target.Sha;
				}
			}
			return submoduleStatus;
		}
	}
}
