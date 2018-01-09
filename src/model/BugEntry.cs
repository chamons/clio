using System;
namespace clio.Model
{
	// Processed result of ParsedCommits
	public struct BugEntry
	{
		public int ID { get; private set; }
		public string Title { get; private set; }
		public string SecondaryTitle { get; private set; }
		public ParsedCommit BugInfo { get; private set; }

		public BugEntry ( int id, string title, string secondaryTitle, ParsedCommit bugInfo)
		{
			ID = id;
			Title = title.Replace ('`', '`');
			SecondaryTitle = secondaryTitle.Replace ('`', '`');
			BugInfo = bugInfo;
		}
	}
}
