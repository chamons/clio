using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using clio.Model;

namespace clio
{
	public static class CommitParser
	{
        /// <summary>
        /// Parses and validates bugs mentioned in commit messages
        /// </summary>
        public static Task<IList<ParsedCommit>> ParseAndValidateAsync (IEnumerable<CommitInfo> commits, SearchOptions options)
		{
                // TODO: use options to determine which commit parsers to use
                // TODO: use options to determine whether to validate issues found or not

            var vstsIssues = commits.SelectMany(commit => VstsCommitParser.Instance.ParseSingle(commit));
            var bzIssues = commits.SelectMany(commit => BugzillaCommitParser.Instance.ParseSingle(commit));

            return vstsIssues
                .Concat(bzIssues)
                .ValidateAsync(options);

                // VSTS

                //foreach (var parsedCommit in VstsCommitParser.Instance.ParseSingle(commit).Validate(options))
                //{
                //    //if (options.Bugzilla != BugzillaLevel.Disable)
                //    //VstsCommitParser.Instance.DetermineConfidence(parsedCommit);

                //    yield return parsedCommit;
                //}

                //// Bugzilla
                //foreach (var parsedCommit in BugzillaCommitParser.Instance.ParseSingle(commit).Validate(options))
                //{
                //    //if (options.Bugzilla != BugzillaLevel.Disable)
                //    //BugzillaCommitParser.Instance.DetermineConfidence(parsedCommit);

                //    yield return parsedCommit;
                //}

                // TODO: GitHub issues
		}

        public static async Task<IList<ParsedCommit>> ValidateAsync(this IEnumerable<ParsedCommit> commits, SearchOptions options)
        {
            var vsts = new VstsChecker(options);
            var bz = new BugzillaChecker(options);

            await bz.Setup().ConfigureAwait (false);

            // fire off tasks to validate all the commit issues
            // each task will return a new parsed commit with an issue assigned with a confidence

            var tasks = new Task<IEnumerable<ParsedCommit>>[] { vsts.ValidateIssuesAsync(commits), bz.ValidateIssuesAsync(commits) };
            await Task.WhenAll(tasks).ConfigureAwait (false);

            return tasks.SelectMany(t => t.Result).ToList();
        }
	}
}
 