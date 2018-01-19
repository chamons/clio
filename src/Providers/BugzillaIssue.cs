using clio.Model;
using CodeRinseRepeat.Bugzilla;

namespace clio.Providers
{
	public sealed class BugzillaIssue : IIssue
	{
		public IssueSource IssueSource => IssueSource.Bugzilla;

		public int Id { get; }
		public string Title { get; }
		public string MoreInfo { get; }
		public string TargetMilestone { get; }
		public string Status { get; }
		public string Importance { get; }
		public bool IsEnhancement { get; }
		public string IssueUrl { get; }
		public bool IsClosed { get; }

		public BugzillaIssue (Bug bug)
		{
			Id = (int)bug.Id;
			Title = bug.Summary;
			MoreInfo = $"({bug.Product}) - {bug.Milestone} {bug.Status}";
			TargetMilestone = bug.Milestone;
			Status = bug.Status;
			Importance = bug.Severity;
			IsEnhancement = bug.Severity == "enhancement";
			IssueUrl = $"https://bugzilla.xamarin.com/show_bug.cgi?id={this.Id}";

			switch (bug.Status) 
			{
				case "CLOSED":
				case "VERIFIED":
				case "RESOLVED":
					IsClosed = true;
					break;
				default:
					IsClosed = false;
					break;
			}
		}
	}
}
