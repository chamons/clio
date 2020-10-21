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
		Regex BackportExpression = new Regex (@"Backport of #(\d*)", RegexOptions.Compiled);

		public async Task<RequestCollection> FindPullRequests (string location, bool useCache, IEnumerable<CommitInfo> commits)
		{
			var allIssues = await IssueCache.GetIssues (this.Client, location, useCache);

			var requests = new RequestCollection (allIssues);

			foreach (var commit in commits) {

				string prMatch = TryExpression (SquashExpression, commit);

				if (prMatch == null)
					prMatch = TryExpression (MergeExpression, commit);
				
				if (prMatch != null && Int32.TryParse (prMatch, out int id)) {
					var matchPR = allIssues.FirstOrDefault (x => x.Number == id);

					if (matchPR != null) 
					{
						var labels = matchPR.Labels;
						if (labels.Count == 0) {
							var backport = TryBodyExpression(BackportExpression, matchPR.Body);
							if (backport != null && Int32.TryParse (backport, out int backportID))
							{
								var matchBackport = allIssues.FirstOrDefault (x => x.Number == backportID);
								if (matchBackport != null) 
								{
									labels = matchBackport.Labels;
								}
							}
						}
						if (!labels.Any (x => x == "not-notes-worthy")) {
							requests.Add (new RequestInfo (matchPR.Number, string.Format ("{0:MM/dd/yyyy}", matchPR.ClosedAt), commit.Title, 
								commit.Description, matchPR.Title, matchPR.Body, commit.Hash, commit.Author, matchPR.Url, 
								labels.Where (x => IsInterestingLabel (x))));
						}
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

		string TryBodyExpression (Regex expression, string body)
		{
			var match = expression.Match (body);
			if (match.Success)
				return match.Groups[1].Value;
			return null;
		}
	}
}
