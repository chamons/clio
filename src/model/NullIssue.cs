﻿namespace clio.Model
{
	public sealed class NullIssue : IIssue
	{
		public NullIssue (IssueSource issueSource, int id)
		{
			this.IssueSource = issueSource;
			this.Id = id;
			this.Title = string.Empty;
			this.MoreInfo = string.Empty;
			this.TargetMilestone = string.Empty;
			this.Status = string.Empty;
			this.Importance = string.Empty;
		}

		public IssueSource IssueSource { get; }

		public int Id { get; }

		public string Title { get; }

		public string MoreInfo { get; }

		public string TargetMilestone { get; }

		public string Status { get; }

		public string Importance { get; }

		public bool IsEnhancement => false;

		public string IssueUrl => string.Empty;

		public bool IsClosed => false;
	}
}
