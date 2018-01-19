using clio.Model;

namespace clio.Providers.Issues
{
	public sealed class VstsIssue : IIssue
	{
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

		public VstsIssue (int issueId, VisualStudioBug bug)
		{
			Id = issueId;
			Title = bug.Fields ["System.Title"];
			MoreInfo = $"{bug.Fields["System.AreaPath"]} - {bug.Fields["Microsoft.DevDiv.Milestone"]} {bug.Fields["System.State"]}";
			TargetMilestone = bug.Fields["Microsoft.DevDiv.Milestone"];
			Status = bug.Fields["System.State"];
			Importance = bug.Fields["Microsoft.VSTS.Common.Priority"];

			// TODO: is UserStory the correct or only workitem type we want to call an enhancement?
			IsEnhancement = bug.Fields["System.WorkItemType"] == "UserStory";
			IssueUrl = $"https://devdiv.visualstudio.com/DevDiv/_workitems/edit/{this.Id}";

			// TODO: is "closed" the only status to define a bug / work item as closed?
			IsClosed = this.Status == "Closed";
		}
	}
}
