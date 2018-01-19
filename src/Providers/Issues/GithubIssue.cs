using System;
using clio.Model;

namespace clio.Providers.Issues
{
	public class GithubIssue : IIssue
	{
		public IssueSource IssueSource => IssueSource.GitHub;

		public int Id { get; }
		public string Title { get; }
		public string MoreInfo { get; }
		public string TargetMilestone { get; }
		public string Status { get; }
		public string Importance { get; }
		public bool IsEnhancement { get; }
		public string IssueUrl { get; }
		public bool IsClosed { get; }

		public GithubIssue (Octokit.Issue issue)
		{
			Id = issue.Id;
			Title = issue.Title;
			MoreInfo = issue.Body;
			TargetMilestone = "";
			Status = issue.State == Octokit.ItemState.Closed ? "Closed" : "Open";
			Importance = "";
			IsEnhancement = false;
			IssueUrl = issue.Url;
			IsClosed = issue.State == Octokit.ItemState.Closed;
		}
	}
}
