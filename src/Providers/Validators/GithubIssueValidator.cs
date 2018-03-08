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
			try
			{
				var githubIssue = await Client.Issue.Get (Owner, Area, issueId);
				await Task.Delay (250);
				return new GithubIssue (githubIssue);
			}
			catch (NotFoundException)
			{
				return null;
			}
			catch (RateLimitExceededException e)
			{
				EntryPoint.Die ($"Github has rate limited your IP. Pass --github-pat or wait until {e.Reset}");
				return null;
			}
		}

		protected override Task SetupAsync ()
		{
			Client = new GitHubClient (new ProductHeaderValue ("chamons.clio"));
			if (Options.GithubPAT != null)
				Client.Credentials = new Credentials (Options.GithubPAT);
			else
				Explain.Print ("Using anon github. You are very likely to be rate limited. Create a PAT and pass --github-pat");
			return Task.CompletedTask;
		}
	}
}
