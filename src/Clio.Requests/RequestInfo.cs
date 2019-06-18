using System;
using System.Diagnostics;

namespace Clio.Requests
{
	public struct ItemInfo
	{
		public string Title;
		public string Description;

		public ItemInfo (string title, string description)
		{
			Title = title;
			Description = description;
		}
	}

	[DebuggerDisplay("{ID} - {PRInfo.Title}")]
	public struct RequestInfo 
	{
		public int ID;
		public string Date;
		public ItemInfo CommitInfo;
		public ItemInfo PRInfo;
		public string Hash;
		public string Author;
		public string URL;

		public RequestInfo (int id, string date, string commitTitle, string commitDescription, string prTitle, string prDescription, string hash, string author, string url)
		{
			ID = id;
			Date = date;
			CommitInfo = new ItemInfo (commitTitle, commitDescription);
			PRInfo = new ItemInfo (prTitle, prDescription);
			Hash = hash;
			Author = author;
			URL = url;
		}
	}
}
