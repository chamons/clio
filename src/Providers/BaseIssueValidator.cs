using System.Collections.Generic;
using System.Threading.Tasks;
using clio.Model;

namespace clio.Providers
{
	/// <summary>
	/// Base issue validation implementation
	/// </summary>
	public abstract class BaseIssueValidator : IIssueValidator
	{
		public BaseIssueValidator (IssueSource issueSource, SearchOptions options)
		{
			this.IssueSource = issueSource;
			this.Options = options;
		}

		protected IssueSource IssueSource { get; private set; }

		protected SearchOptions Options { get; private set; }

		protected abstract Task SetupAsync ();

		/// <summary>
		/// Validates commits and assigns an issue to the commit if validated.
		/// </summary>
		public async Task<IEnumerable<ParsedCommit>> ValidateIssuesAsync (IEnumerable<ParsedCommit> commits)
		{
			await this.SetupAsync ().ConfigureAwait (false);

			var results = new List<ParsedCommit> ();
			foreach (var commit in commits)
			{
				// we will only check issues that match our source
				if (commit.IssueSource == this.IssueSource)
				{
					var issue = await this.GetIssueAsync (commit.IssueId).ConfigureAwait (false);
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
				else
				{
					// do we actually need to do this, can we just skip issues that are not
					// for us?
					results.Add (new ParsedCommit (commit, commit.Confidence));
				}
			}

			return results;
		}

		public abstract Task<IIssue> GetIssueAsync (int issueId);
	}
}
