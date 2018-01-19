using System.Collections.Generic;
using System.Threading.Tasks;
using clio.Model;

namespace clio.Providers.Validators
{
	public interface IIssueValidator
	{
		// Returns a new enumerable where the confidence of the parsed commit is adjusted up or down.
		Task<IEnumerable<ParsedCommit>> ValidateIssuesAsync (IEnumerable<ParsedCommit> commits);
	}

	public abstract class BaseIssueValidator : IIssueValidator
	{
		protected IssueSource IssueSource { get; private set; }
		protected SearchOptions Options { get; private set; }

		public BaseIssueValidator (IssueSource issueSource, SearchOptions options)
		{
			IssueSource = issueSource;
			Options = options;
		}

		protected abstract Task SetupAsync ();
		public abstract Task<IIssue> GetIssueAsync (int issueId);

		/// <summary>
		/// Validates commits and assigns an issue to the commit if validated.
		/// </summary>
		public async Task<IEnumerable<ParsedCommit>> ValidateIssuesAsync (IEnumerable<ParsedCommit> commits)
		{
			await SetupAsync ().ConfigureAwait (false);

			var results = new List<ParsedCommit> ();
			foreach (var commit in commits)
			{
				// we will only check issues that match our source and drop any that are not for us
				// if we return issues not for us then we end up creating duplicates when we merge the
				// result sets
				if (commit.IssueSource == IssueSource)
				{
					var issue = await GetIssueAsync (commit.IssueId).ConfigureAwait (false);
					if (issue != null)
					{
						// can we get the issue, lets update our confidence
						results.Add (new ParsedCommit (commit, issue, ParsingConfidence.High));
					}
					else
					{
						// it's a bug, we think, but we can't identify it
						results.Add (new ParsedCommit (commit, ParsingConfidence.Low));
					}
				}
			}

			return results;
		}
	}

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
