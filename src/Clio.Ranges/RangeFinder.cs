using System;
using System.Collections.Generic;

namespace Clio.Ranges 
{
	public static class RangeFinder 
	{
		public static IEnumerable<CommitInfo> Find (string path, SearchOptions options, ISearchRange range)
		{
			if (range is HashSearchRange hashSearchRange)
				return CommitFinder.ParseHashRange (path, options, hashSearchRange.Oldest, hashSearchRange.Newest);
			else if (range is BranchSearchRange branchSearchRange)
				return CommitFinder.ParseBranchRange (path, options, branchSearchRange.Base, branchSearchRange.Branch);
			else if (range is SingleHashSearchRange singleRange)
				return CommitFinder.ParseSingle (path, options, singleRange.Hash);
			else
				throw new NotImplementedException ();
        }
    }
}