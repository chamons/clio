using System;
namespace clio.Model
{
	public enum IssueSource
	{
		Bugzilla,
		Vsts,
		GitHub
	}

	/// <summary>
	/// Represents an issue in an issue tracker system, ie bugzilla, vsts or github
	/// </summary>
	public interface IIssue
	{
		IssueSource IssueSource { get; }
		int Id { get; }
		string Title { get; }
		string MoreInfo { get; }
		string TargetMilestone { get; }
		string Status { get; }
		string Importance { get; }
		bool IsEnhancement { get; }
		string IssueUrl { get; }
		bool IsClosed { get; }
	}

	public sealed class NullIssue : IIssue
	{
		public NullIssue (IssueSource issueSource, int id)
		{
			IssueSource = issueSource;
			Id = id;
			Title = string.Empty;
			MoreInfo = string.Empty;
			TargetMilestone = string.Empty;
			Status = string.Empty;
			Importance = string.Empty;
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
