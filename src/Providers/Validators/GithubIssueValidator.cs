using System;
using System.Threading.Tasks;
using clio.Model;

namespace clio.Providers.Validators
{
	public class GithubIssueValidator : BaseIssueValidator
	{
		public GithubIssueValidator (SearchOptions options) : base (IssueSource.GitHub, options)
		{
		}

		public override Task<IIssue> GetIssueAsync (int issueId)
		{
			throw new NotImplementedException ();
		}

		protected override Task SetupAsync ()
		{
			throw new NotImplementedException ();
		}
	}
}
