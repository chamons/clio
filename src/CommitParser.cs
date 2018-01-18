using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using clio.Model;
using clio.Providers;

namespace clio
{
	public static class CommitParser
	{
		/// <summary>
		/// Parses and validates bugs mentioned in commit messages
		/// </summary>
		public static Task<IEnumerable<ParsedCommit>> ParseAndValidateAsync (IEnumerable<CommitInfo> commits, SearchOptions options)
		{
			// enumerate these now
			var allCommits = commits.ToList ();

			// grab all the issues that we can find (these will all be >= Low confidence
			var parsedCommits = GetCommitParsers (options)
				.SelectMany (parser => allCommits.SelectMany (commit => parser.ParseSingle (commit)))
				.ToList ();

			return parsedCommits
				.ValidateAsync (options);
		}

		/// <summary>
		/// Validates the commits and returns a new enumerable containing updated confidences for each commit.
		/// </summary>
		public static async Task<IEnumerable<ParsedCommit>> ValidateAsync (this IEnumerable<ParsedCommit> commits, SearchOptions options)
		{
			var tasks = new List<Task<IEnumerable<ParsedCommit>>> ();

			// enumerate these now
			var allCommits = commits.ToList ();

			// fire off validation tasks for each of the validators for each of the commits
			foreach (var validator in GetValidators (options))
			{
				tasks.Add (validator.ValidateIssuesAsync (allCommits));
			}

			// await everything
			await Task.WhenAll (tasks).ConfigureAwait (false);

			// unfold into a single list
			return tasks.SelectMany (t => t.Result).ToList ();
		}

		static IEnumerable<ICommitParser> GetCommitParsers (SearchOptions options)
		{
			if (!options.IgnoreVsts)
				yield return VstsCommitParser.Instance;
			if (!options.IgnoreBugzilla)
				yield return BugzillaCommitParser.Instance;
		}

		static IEnumerable<IIssueValidator> GetValidators (SearchOptions options)
		{
			if (options.Vsts != VstsLevel.Disable && !options.IgnoreVsts) {
				yield return new VstsIssueValidator (options);
			} else {
				yield return new DefaultIssueValidator (IssueSource.Vsts, options);
			}

			if (options.Bugzilla != BugzillaLevel.Disable && !options.IgnoreBugzilla) {
				yield return new BugzillaIssueValidator (options);
			} else {
				yield return new DefaultIssueValidator (IssueSource.Bugzilla, options);
			}
		}
	}
}
