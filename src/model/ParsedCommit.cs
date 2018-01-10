using System;
namespace clio.Model
{
	/// <summary>
	/// A commit that has been parsed and associated with an issue found in the commit message
	/// </summary>
	public struct ParsedCommit
	{
		/// <summary>
		/// Gets the source of the issue that was found in the commit
		/// </summary>
		public IssueSource IssueSource { get; }

		/// <summary>
		/// Gets the commit info for the commit
		/// </summary>
		public CommitInfo Commit { get; }

		/// <summary>
		/// Gets the link that was used to denote the bug in the commit
		/// </summary>
		public string Link { get; }

		/// <summary>
		/// Gets the IssueId that was found in the commit
		/// </summary>
		public int IssueId { get; }

		/// <summary>
		/// Gets a value indicating the level of confidence that the issue does indeed relate to the commit.
		/// </summary>
		public ParsingConfidence Confidence { get; }

		/// <summary>
		/// Gets the issue that was found with IssueId from the IssueSource
		/// </summary>
		public IIssue Issue { get; }

		public ParsedCommit (IssueSource issueSource, CommitInfo commit, string link, int id, ParsingConfidence confidence)
		{
			IssueSource = issueSource;
			Commit = commit;
			Link = link;
			IssueId = id;
			Confidence = confidence;
			Issue = NullIssue.Instance;
		}

		public ParsedCommit (ParsedCommit commit, IIssue issue, ParsingConfidence confidence)
		{
			IssueSource = commit.IssueSource;
			Commit = commit.Commit;
			Link = commit.Link;
			IssueId = commit.IssueId;
			Confidence = confidence;
			Issue = issue ?? NullIssue.Instance;
		}

		public ParsedCommit (ParsedCommit commit, ParsingConfidence confidence)
		{
			IssueSource = commit.IssueSource;
			Commit = commit.Commit;
			Link = commit.Link;
			IssueId = commit.IssueId;
			Confidence = confidence;
			Issue = commit.Issue ?? NullIssue.Instance;
		}
	}
}
