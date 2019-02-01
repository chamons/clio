using System;

namespace clio.Model
{
	/// <summary>
	/// The processed result of a ParsedCommit
	/// </summary>
	public struct BugEntry
	{
		public int Id { get; private set; }

		public string Title { get; private set; }

		public string SecondaryTitle { get; private set; }

		public IIssue IssueInfo { get; private set; }

		public IssueSource IssueSource { get; private set; }

		public string Date { get; private set; }

		public ParsedCommit Commit { get; private set; }

		public BugEntry (ParsedCommit parsedCommit)
		{
			Id = parsedCommit.IssueId;
			Title = parsedCommit.Issue.Title?.Replace ('`', '`') ?? string.Empty;
			SecondaryTitle = parsedCommit.Commit.Title?.Replace ('`', '`') ?? string.Empty;
			IssueInfo = parsedCommit.Issue;
			IssueSource = parsedCommit.IssueSource;
			Date = parsedCommit.Commit.CommitDate;
			Commit = parsedCommit;
		}
	}
}
