using System.Collections.Generic;
using System.Threading.Tasks;
using clio.Model;

namespace clio.Providers
{
    public abstract class BaseIssueValidator : IIssueValidator
    {
        public BaseIssueValidator(IssueSource issueSource, SearchOptions options)
        {
            this.IssueSource = issueSource;
            this.Options = options;
        }

        protected IssueSource IssueSource { get; private set; }

        protected SearchOptions Options { get; private set; }

        public abstract Task SetupAsync();

        /// <summary>
        /// Validates commits and assigns an issue to the commit if validated.
        /// </summary>
        public async Task<IEnumerable<ParsedCommit>> ValidateIssuesAsync(IEnumerable<ParsedCommit> commits)
        {
            var results = new List<ParsedCommit>();
            foreach (var commit in commits)
            {
                if (commit.IssueSource == this.IssueSource)
                {
                    var issue = await this.GetIssueAsync(commit);
                    if (issue != null)
                    {
                        results.Add(new ParsedCommit(commit, issue, ParsingConfidence.High));
                    }
                    else
                    {
                        // it's a bug, we think, but we can't identify it
                        results.Add(new ParsedCommit(commit, ParsingConfidence.Low));
                    }
                }
            }

            return results;
        }

        protected abstract Task<IIssue> GetIssueAsync(ParsedCommit commit);
    }
}
