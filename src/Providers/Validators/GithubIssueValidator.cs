using System.Threading.Tasks;

using clio.Model;
using clio.Providers.Issues;
using Octokit;

namespace clio.Providers.Validators
{
	public class GithubIssueValidator : BaseIssueValidator
	{
		GitHubClient Client;
		string Owner;
		string Area;

		public GithubIssueValidator (SearchOptions options) : base (IssueSource.GitHub, options)
		{
			var bits = options.GithubLocation.Split ('/');
			if (bits.Length != 2)
				EntryPoint.Die ("--github formatted incorrectly");
			
			Owner = bits[0];
			Area = bits[1];
		}

		public override async Task<IIssue> GetIssueAsync (int issueId)
		{
			var githubIssue = await Client.Issue.Get (Owner, Area, issueId);

			return new GithubIssue (githubIssue);
		}

		protected override Task SetupAsync ()
		{
			Client = new GitHubClient (new ProductHeaderValue ("chamons.clio"));
			return Task.CompletedTask;
		}
	}
}
