using System.Threading.Tasks;
using clio.Model;

namespace clio.Providers
{
	public class DefaultIssueValidator : BaseIssueValidator
	{
		public DefaultIssueValidator (IssueSource source, SearchOptions options) : base (source, options)
		{
		}

		protected override Task SetupAsync ()
		{
			return Task.CompletedTask;
		}

		public override Task<IIssue> GetIssueAsync (int issueId)
		{
			return Task.FromResult<IIssue> (null);
		}
	}
}
