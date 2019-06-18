using System;

namespace Clio.Ranges 
{
	// Raw data harvested from git
	public struct CommitInfo
	{
		public string Hash { get; private set; }
		public string Title { get; private set; }
		public string Description { get; private set; }
		public string Author { get; private set; }
		public string CommitDate { get; private set; }

		public CommitInfo (string hash, string title, string description, string commitDate, string author)
		{
			Hash = hash;
			Title = title;
			Description = description;
			CommitDate = commitDate;
			Author = author;
		}

		public CommitInfo (string hash, string title, string description) : this (hash, title, description, "", "")
		{
		}
	}
}