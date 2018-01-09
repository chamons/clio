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
		public static IEnumerable<ParsedCommit> ParseAndValidate (IEnumerable<CommitInfo> commits, SearchOptions options)
		{
                // TODO: use options to determine which commit parsers to use
                // TODO: use options to determine whether to validate issues found or not

            var vstsIssues = commits.SelectMany(commit => VstsCommitParser.Instance.ParseSingle(commit));
            var bzIssues = commits.SelectMany(commit => BugzillaCommitParser.Instance.ParseSingle(commit));

            return vstsIssues
                .Concat(bzIssues)
                .Validate(options);

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

        public static IEnumerable<ParsedCommit> Validate(this IEnumerable<ParsedCommit> commits, SearchOptions options)
        {
            var vsts = new VstsChecker(options);
            var bz = new BugzillaChecker(options);

            if (options.Bugzilla != BugzillaLevel.Private)
                bz.Setup().Wait();

            // fire off tasks to validate all the commit issues
            // each task will return a new parsed commit with an issue assigned with a confidence

            var tasks = new Task<IEnumerable<ParsedCommit>>[] { vsts.ValidateIssuesAsync(commits), bz.ValidateIssuesAsync(commits) };
            Task.WaitAll(tasks);

            foreach (var commit in tasks.SelectMany(t => t.Result))
            {
                yield return commit;
            }
        }
	}
}
 