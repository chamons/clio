using System;
namespace clio.Model
{
	public enum ParsingConfidence
	{
		High, // Clearly appears to be a fixed bug
		Low, // A bug, but unclear based on context
		Invalid, // None found
	}

	// CommitInfo with metadata from bugzilla and heuristics applied 
	public struct ParsedCommit
	{
		public CommitInfo Commit { get; }
		public string Link { get; }
		public int ID { get; }
		public ParsingConfidence Confidence { get; }
		public string BugzillaSummary { get; }
		public string TargetMilestone { get; }
		public string Status { get; }
		public string Importance { get; }

		public ParsedCommit (CommitInfo commit, string link, int id, ParsingConfidence confidence, string bugzillaSummary, string targetMilestone, string status, string importance)
		{
			Commit = commit;
			Link = link;
			ID = id;
			Confidence = confidence;
			BugzillaSummary = bugzillaSummary;
			TargetMilestone = targetMilestone;
			Status = status;
			Importance = importance;
		}
	}
}
