using System.Collections.Generic;

namespace Clio.Ranges
{
	public interface ISearchRange {}

	public class HashSearchRange : ISearchRange
	{
		public string Oldest { get; set; }
		public string Newest { get; set; }
	}

	public class BranchSearchRange : ISearchRange
	{
		public string Base { get; set; }	
		public string Branch { get; set; }
	}

	public class SingleHashSearchRange : ISearchRange
	{
		public string Hash { get; set; }
	}

	public class SearchOptions
	{
		public string GithubLocation { get; set; }
		public string GithubPAT { get; set; }

		public List<string> CommitsToIgnore { get; set; } = new List<string> ();
	}
}