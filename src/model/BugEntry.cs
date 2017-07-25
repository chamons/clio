using System;
namespace clio.Model
{
	// Processed result of ParsedCommits
	public struct BugEntry
	{
		public int ID { get; private set; }
		public string Title { get; private set; }
		public string SecondaryTitle { get; private set; }

		// TODO - Figure out PR in question
		public string PR { get; private set; }

		public BugEntry (int id, string title, string secondaryTitle, string pr = "")
		{
			ID = id;
			Title = title;
			SecondaryTitle = secondaryTitle;
			PR = pr;
		}
	}
}
