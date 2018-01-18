using clio.Model;

namespace clio.Providers
{
	public sealed class VstsIssue : IIssue
	{
		public VstsIssue (int issueId, VisualStudioBug bug)
		{
			this.Id = issueId;
			this.Title = bug.Fields ["System.Title"];
			this.MoreInfo = $"{bug.Fields["System.AreaPath"]} - {bug.Fields["Microsoft.DevDiv.Milestone"]} {bug.Fields["System.State"]}";
			this.TargetMilestone = bug.Fields["Microsoft.DevDiv.Milestone"];
			this.Status = bug.Fields["System.State"];
			this.Importance = bug.Fields["Microsoft.VSTS.Common.Priority"];
			// TODO: is UserStory the correct or only workitem type we want to call an enhancement?
			this.IsEnhancement = bug.Fields["System.WorkItemType"] == "UserStory";
			this.IssueUrl = $"https://devdiv.visualstudio.com/DevDiv/_workitems/edit/{this.Id}";
			// TODO: is "closed" the only status to define a bug / work item as closed?
			this.IsClosed = this.Status == "Closed";
		}

		public IssueSource IssueSource => IssueSource.Vsts;

		public int Id { get; }

		public string Title { get; }

		public string MoreInfo { get; }

		public string TargetMilestone { get; }

		public string Status { get; }

		public string Importance { get; }

		public bool IsEnhancement { get; }

		public string IssueUrl { get; }

		public bool IsClosed { get; }
	}
}
