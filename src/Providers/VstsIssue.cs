using clio.Model;

namespace clio.Providers
{
	public sealed class VstsIssue : IIssue
	{
		public VstsIssue (int issueId)
		{
			// TODO: VstsIssue
			this.Id = issueId;
			this.Title = string.Empty;
			this.MoreInfo = string.Empty;
			this.TargetMilestone = string.Empty;
			this.Status = string.Empty;
			this.Importance = string.Empty;
			this.IsEnhancement = false;
			this.IssueUrl = $"https://devdiv.visualstudio.com/DevDiv/_workitems/edit/{this.Id}";
			this.IsClosed = false;
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
