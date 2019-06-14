using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Octokit;

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

        Task<SearchIssuesResult> FindPR (string commit)
        {
                var request = new SearchIssuesRequest ("SHA " + commit) {
                    Type = IssueTypeQualifier.PullRequest,
                };
                return Client.Search.SearchIssues (request);
        }

        public async Task<RequestCollection> FindPullRequests (string location, IEnumerable<string> commits)
        {
            var (owner, area) = ParseLocation (location);

            var requests = new RequestCollection ();
            foreach (var commit in commits) {
                var commitInfo = await Client.Repository.Commit.Get (owner, area, commit);
                var pr = await FindPR (commit);

                var commitLines = commitInfo.Commit.Message.SplitLines ();
                string commitMessage = commitLines.First (); 
                string commitDescription = string.Join ("", commitLines.Skip (1));
                if (pr.Items.Count != 1)
                    Errors.Die ($"Found {pr.Items.Count} items for hash {commit} not 1");
                var prItem = pr.Items[0];
                requests.Add (new RequestInfo (prItem.Id, prItem.ClosedAt.ToString (), commitMessage, commitDescription, prItem.Title, prItem.Body, commit, commitInfo.Commit.Author.Email, prItem.Url));
            }
            return requests;
        }
    }
}
