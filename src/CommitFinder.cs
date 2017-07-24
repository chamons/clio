using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using LibGit2Sharp;
using Optional;

namespace clio
{
	public struct CommitInfo
	{
		public string Hash { get; private set; }
		public string Title { get; private set; }
		public string Description { get; private set; }

		public CommitInfo (string hash, string title, string description)
		{
			Hash = hash;
			Title = title;
			Description = description;
		}
	}
	
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
	}
}
