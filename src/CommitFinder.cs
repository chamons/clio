using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using clio.Model;
using LibGit2Sharp;
using Optional;
using Optional.Unsafe;

namespace clio
{
	public static class CommitFinder
	{
		static CommitInfo CreateInfoFromCommit (Commit x) => new CommitInfo (x.Sha, x.MessageShort, x.Message, x.Author.Email);

		public static IEnumerable<CommitInfo> ParseHashRange (string path, SearchOptions options, HashSearchRange range)
		{
			try {
				using (var repo = new Repository (path))
				{
					List<CommitInfo> commitInfo = new List<CommitInfo> ();

					CommitFilter filter = CreateCommitFilter (options, range);

					foreach (var commit in repo.Commits.QueryBy (filter)) {
						if (options.CommitsToIgnore.Contains (commit.Id.ToString ()))
							continue;

						commitInfo.Add (CreateInfoFromCommit (commit));
					}
					return commitInfo;
				}
			}
			catch (RepositoryNotFoundException) {
				return Enumerable.Empty<CommitInfo> ();
			}
			catch (NotFoundException) {
				return Enumerable.Empty<CommitInfo> ();
			}
		}

		public static IEnumerable<CommitInfo> FindMergeCommits (string path, SearchOptions options, HashSearchRange range)
		{
			try {
				using (var repo = new Repository (path)) {
					List<CommitInfo> commitInfo = new List<CommitInfo> ();

					CommitFilter filter = CreateCommitFilter (options, range);

					foreach (var commit in repo.Commits.QueryBy (filter)) {			
						// Commits with more than one parent are merge commits
						if (commit.Parents.Count () > 1) {
							commitInfo.Add (CreateInfoFromCommit (commit));
						}
					}
					return commitInfo;
				}
			}
			catch (RepositoryNotFoundException) {
				return Enumerable.Empty<CommitInfo> ();
			}
			catch (NotFoundException){
				return Enumerable.Empty<CommitInfo> ();
			}
		}

		static CommitFilter CreateCommitFilter (SearchOptions options, HashSearchRange range)
		{
			var filter = new CommitFilter { ExcludeReachableFrom = range.Oldest, IncludeReachableFrom = range.Newest };

			if (options.MergeCommitsToIgnore.Any ()) {
				filter.ExcludeReachableFrom = options.MergeCommitsToIgnore;
			}
			return filter;
		}

		public static IEnumerable<CommitInfo> ParseBranchRange (string path, SearchOptions options, string baseBranch, string branch)
		{
			using (var repo = new Repository (path)) {
				string cherryArguments = $"-v {branch} {baseBranch}";
				StringBuilder stringBuilder = new StringBuilder ();
				int cherryReturnValue = RunCommand.Run ("/usr/local/bin/git", $"cherry {cherryArguments}", path, output: stringBuilder);
				if (cherryReturnValue != 0)
					EntryPoint.Die ($"git cherry returned {cherryReturnValue} with arguments {cherryArguments}");
				foreach (var line in stringBuilder.ToString ().Split (new string[] { Environment.NewLine }, StringSplitOptions.None))
				{
					if (line.StartsWith ("+", StringComparison.Ordinal)) {
						string hash = line.Split (' ')[1];
						if (!options.CommitsToIgnore.Contains (hash)) {
							var commit = repo.Lookup<Commit> (hash);
							yield return CreateInfoFromCommit (commit);
						}
					}
				}
			}
			yield break;
		}
	}
}
