using System;

namespace clio.Model
{
	// Raw data harvested from git
	public struct CommitInfo
	{
		public string Hash { get; private set; }
		public string Title { get; private set; }
		public string Description { get; private set; }
		public string Author { get; private set; }

		public CommitInfo (string hash, string title, string description, string author = "")
		{
			Hash = hash;
			Title = title;
			Description = description;
			Author = author;
		}
	}
}