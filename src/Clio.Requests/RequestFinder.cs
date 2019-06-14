using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

        public async Task<RequestCollection> FindPullRequests (string location, IEnumerable<CommitInfo> commits)
        {
            var (owner, area) = ParseLocation (location);

            var requests = new RequestCollection ();

            var allPRs = await Client.Search.SearchIssues (new SearchIssuesRequest () {
                Type = IssueTypeQualifier.PullRequest,
                Repos = new RepositoryCollection () { location }
            });

            foreach (var commit in commits) {
                // There are zero items here, where oh where is the merge Commit!?
                var prItem = allPRs.Items.FirstOrDefault (x => x.PullRequest.MergeCommitSha == commit.Hash);
                if (prItem != null)
                    requests.Add (new RequestInfo (prItem.Id, string.Format ("{0:MM/dd/yyyy}", prItem.ClosedAt), commit.Title, 
                        commit.Description, prItem.Title, prItem.Body, commit.Hash, commit.Author, prItem.Url));
            }
            return requests;
        }
    }
}
