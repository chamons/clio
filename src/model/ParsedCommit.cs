using System;
namespace clio.Model
{
	public enum ParsingConfidence
	{
		High, // Clearly appears to be a fixed bug
		Low, // A bug, but unclear based on context
		Invalid, // None found
	}

	public struct ParsedCommit
	{
		public CommitInfo Commit { get; private set; }
		public string Link { get; private set; }
		public ParsingConfidence Confidence { get; private set; }

		public ParsedCommit (CommitInfo commit, string link, ParsingConfidence confidence)
		{
			Commit = commit;
			Link = link;
			Confidence = confidence;
		}
	}
}
