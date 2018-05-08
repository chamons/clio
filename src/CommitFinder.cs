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

		public static IEnumerable<CommitInfo> ParseHashRange (string path, SearchOptions options, string oldest, string newest)
		{
			var filter = new CommitFilter { ExcludeReachableFrom = oldest, IncludeReachableFrom = newest };
			return ParseWithFilter (path, options, filter);
		}

		static IEnumerable<CommitInfo> ParseWithFilter (string path, SearchOptions options, CommitFilter filter)
		{
			try {
				using (var repo = new Repository (path))
					return repo.Commits.QueryBy (filter).Where(x => !options.CommitsToIgnore.Contains (x.Id.ToString ())).Select (x => CreateInfoFromCommit (x)).ToList ();
			}
			catch (RepositoryNotFoundException) {
				return Enumerable.Empty<CommitInfo> ();
			}
			catch (NotFoundException) {
				return Enumerable.Empty<CommitInfo> ();
			}
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
