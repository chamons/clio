using System;

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

	public struct RequestInfo 
    {
        ItemInfo CommitInfo;
        ItemInfo PRInfo;
        string Hash;
        string Author;

        public RequestInfo (string commitTitle, string commitDescription, string prTitle, string prDescription, string hash, string author)
        {
            CommitInfo = new ItemInfo (commitTitle, commitDescription);
            PRInfo = new ItemInfo (prTitle, prDescription);
            Hash = hash;
            Author = author;
        }
	}
}