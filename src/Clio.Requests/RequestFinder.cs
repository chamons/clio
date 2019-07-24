using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

using Octokit;

using Clio.Ranges;
using Clio.Utilities;

namespace Clio.Requests
{
	public class RequestFinder 
	{
		GitHubClient Client;

		public RequestFinder (string pat)
		{
			Client = new GitHubClient (new ProductHeaderValue ("chamons.clio"));
			Client.Credentials = new Credentials (pat);
		}

		(string Owner, string Area) ParseLocation (string location)
		{
			var bits = location.Split ('/');
			if (bits.Length != 2)
				Errors.Die ("--github formatted incorrectly");
			return (bits[0], bits[1]);
		}

		public async Task AssertLimits ()
		{
			var limits = await Client.Miscellaneous.GetRateLimits ();
			int coreLimit = limits.Resources.Core.Remaining;
			int searchLimit = limits.Resources.Search.Remaining;
			if (coreLimit < 1 || searchLimit < 1)
				Errors.Die ($"Rate Limit Hit: {coreLimit} {searchLimit}");
		}

		bool IsInterestingLabel (string label)
		{
			switch (label)
			{
				case "bug":
				case "enhancement":
				case "note-highlight":
				case "note-infrastructure":
					return true; // Top level divisions
				case "breaking-change":
				case "regression":
				case "note-deprecation":
					return true; // Worth calling out as negative impacting
				case "community":
					return true; // Worth calling out as positive!
			}
			return false;
		}

		Regex SquashExpression = new Regex (@"[(]#(\d*)[)]", RegexOptions.Compiled);
		Regex MergeExpression = new Regex (@"Merge pull request #(\d*)", RegexOptions.Compiled);

		public async Task<RequestCollection> FindPullRequests (string location, IEnumerable<CommitInfo> commits)
		{
			var (owner, area) = ParseLocation (location);

			var allIssues = await Client.Issue.GetAllForRepository (owner, area, new RepositoryIssueRequest { State = ItemStateFilter.All });

			var requests = new RequestCollection (allIssues);

			foreach (var commit in commits) {

				string prMatch = TryExpression (SquashExpression, commit);

				if (prMatch == null)
					prMatch = TryExpression (MergeExpression, commit);
				
				if (prMatch != null && Int32.TryParse (prMatch, out int id)) {
					var matchPR = allIssues.FirstOrDefault (x => x.Number == id);

					if (matchPR != null && !matchPR.Labels.Any (x => x.Name == "not-notes-worthy")) {
						requests.Add (new RequestInfo (matchPR.Number, string.Format ("{0:MM/dd/yyyy}", matchPR.ClosedAt), commit.Title, 
							commit.Description, matchPR.Title, matchPR.Body, commit.Hash, commit.Author, matchPR.Url, 
							matchPR.Labels.Where (x => IsInterestingLabel (x.Name)).Select (x => x.Name)));
					}
				}
			}
			return requests;
		}

		string TryExpression (Regex expression, CommitInfo commit)
		{
			var match = expression.Match (commit.Title);
			if (match.Success)
				return match.Groups[1].Value;
			return null;
		}
	}
}
