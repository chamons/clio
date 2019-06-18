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

		Regex PRExpression = new Regex (@"[(]#(\d*)[)]", RegexOptions.Compiled);
		public async Task<RequestCollection> FindPullRequests (string location, IEnumerable<CommitInfo> commits)
		{
			var (owner, area) = ParseLocation (location);

			var requests = new RequestCollection ();

			var allPRs = await Client.Search.SearchIssues (new SearchIssuesRequest () {
				Type = IssueTypeQualifier.PullRequest,
				Repos = new RepositoryCollection () { location }
			});

			foreach (var commit in commits) {
				var match = PRExpression.Match (commit.Title);
				if (match.Success) {
					if (Int32.TryParse (match.Groups[1].Value, out int id)) {
						var matchPR = allPRs.Items.FirstOrDefault (x => x.Number == id);
						if (matchPR != null) {
							requests.Add (new RequestInfo (matchPR.Number, string.Format ("{0:MM/dd/yyyy}", matchPR.ClosedAt), commit.Title, 
								commit.Description, matchPR.Title, matchPR.Body, commit.Hash, commit.Author, matchPR.Url));
						}
					}
				}
			}
			return requests;
		}
	}
}
