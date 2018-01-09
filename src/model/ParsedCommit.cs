using System;
namespace clio.Model
{
    // CommitInfo with metadata from bugzilla and heuristics applied 
    public struct ParsedCommit
    {
        public IssueSource BugSource { get; }
        public CommitInfo Commit { get; }
        public string Link { get; }
        public int IssueId { get; }
        public ParsingConfidence Confidence { get; }

        public IIssue Issue { get; }

        // TODO: remove these fields
        public string IssueSummary { get; }
        public string IssueTargetMilestone { get; }
        public string IssueStatus { get; }
        public string IssueImportance { get; }

        public ParsedCommit(IssueSource bugSource, CommitInfo commit, string link, int id, ParsingConfidence confidence)
        {
            BugSource = bugSource;
            Commit = commit;
            Link = link;
            IssueId = id;
            Confidence = confidence;
            Issue = null;

            IssueSummary = string.Empty;
            IssueTargetMilestone = string.Empty;
            IssueStatus = string.Empty;
            IssueImportance = string.Empty;
        }

        public ParsedCommit(ParsedCommit commit, IIssue issue, ParsingConfidence confidence)
        {
            BugSource = commit.BugSource;
            Commit = commit.Commit;
            Link = commit.Link;
            IssueId = commit.IssueId;
            Confidence = confidence;
            Issue = issue;

            IssueSummary = issue?.Title ?? string.Empty;
            IssueTargetMilestone = string.Empty;
            IssueStatus = string.Empty;
            IssueImportance = string.Empty;
        }

        public ParsedCommit(ParsedCommit commit, ParsingConfidence confidence)
        {
            BugSource = commit.BugSource;
            Commit = commit.Commit;
            Link = commit.Link;
            IssueId = commit.IssueId;
            Confidence = confidence;
            Issue = commit.Issue;

            IssueSummary = commit.Issue?.Title ?? string.Empty;
            IssueTargetMilestone = string.Empty;
            IssueStatus = string.Empty;
            IssueImportance = string.Empty;
        }
    }
}
