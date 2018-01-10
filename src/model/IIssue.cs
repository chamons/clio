using System;
namespace clio.Model
{
	/// <summary>
	/// Represents an issue in an issue tracker system, ie bugzilla, vsts or github
	/// </summary>
	public interface IIssue
	{
		/// <summary>
		/// Gets the issue source for this issue
		/// </summary>
		IssueSource IssueSource { get; }

		/// <summary>
		/// Gets the identifier of the issue
		/// </summary>
		int Id { get; }

		/// <summary>
		/// Gets the title of the issue
		/// </summary>
		string Title { get; }

		/// <summary>
		/// Gets the description of the issue
		/// </summary>
		string MoreInfo { get; }

		/// <summary>
		/// Gets the target milestone of the issue
		/// </summary>
		string TargetMilestone { get; }

		/// <summary>
		/// Gets the status of the issue.
		/// </summary>
		string Status { get; }

		/// <summary>
		/// Gets the importance of the issue
		/// </summary>
		string Importance { get; }
	}
}
