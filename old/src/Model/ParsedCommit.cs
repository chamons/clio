using System;
namespace clio.Model
{
	public enum ParsingConfidence
	{
		/// <summary>
		/// This is clearly a bug for the given issue provider, most likely because it
		/// is referenced via the full url or the prefix for the bug clearly denotes it
		/// as such or the bug was verified to exist for the issue provider.
		/// </summary>
		High,

		/// <summary>
		/// This is probably a bug for the given issue provider but we have not yet verified it
		/// </summary>
		Likely,

		/// <summary>
		/// This looks like a bug but verification failed to identify a match
		/// </summary>
		Low,

		/// <summary>
		/// No bug identified
		/// </summary>
		Invalid,
	}

	/// <summary>
	/// A commit that has been parsed and associated with an issue found in the commit message
	/// </summary>
	public struct ParsedCommit
	{
		public IssueSource IssueSource { get; }
		public CommitInfo Commit { get; }
		public string Link { get; }
		public int IssueId { get; }
		public ParsingConfidence Confidence { get; }

		public IIssue Issue { get; }

		public ParsedCommit (IssueSource issueSource, CommitInfo commit, string link, int id, ParsingConfidence confidence)
		{
			IssueSource = issueSource;
			Commit = commit;
			Link = link;
			IssueId = id;
			Confidence = confidence;
			Issue = new NullIssue (issueSource, id);
		}

		public ParsedCommit (ParsedCommit commit, IIssue issue, ParsingConfidence confidence)
		{
			IssueSource = commit.IssueSource;
			Commit = commit.Commit;
			Link = commit.Link;
			IssueId = commit.IssueId;
			Confidence = confidence;
			Issue = issue ?? new NullIssue (commit.IssueSource, commit.IssueId);
		}

		public ParsedCommit (ParsedCommit commit, ParsingConfidence confidence)
		{
			IssueSource = commit.IssueSource;
			Commit = commit.Commit;
			Link = commit.Link;
			IssueId = commit.IssueId;
			Confidence = confidence;
			Issue = commit.Issue ?? new NullIssue (commit.IssueSource, commit.IssueId);
		}
	}
}
