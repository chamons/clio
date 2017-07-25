using System;
using System.Collections.Generic;
using System.Linq;
using clio.Model;

namespace clio
{
	public class BugCollection
	{
		public List<BugEntry> ConfirmedBugs { get; set; } = new List<BugEntry> ();
		public List<BugEntry> UncertainBugs { get; set; } = new List<BugEntry> ();
	}

	public static class BugCollector
	{
		public static BugCollection ClassifyCommits (IEnumerable<ParsedCommit> commits)
		{
			BugCollection collection = new BugCollection ();
			var handledBugs = new HashSet<int> ();

			foreach (var parsedCommit in commits)
			{
				if (handledBugs.Contains (parsedCommit.ID))
				{
					// Commits are either high or low to get here. If we're low
					// then we do not need handling. Either before was low and 
					// we are good, or it was high and we are good
					if (parsedCommit.Confidence != ParsingConfidence.High)
						continue;

					// If we were low before, remove and replace with high
					if (collection.UncertainBugs.Any (x => x.ID == parsedCommit.ID))
					{
						collection.UncertainBugs.RemoveAll (x => x.ID == parsedCommit.ID);
						collection.ConfirmedBugs.Add (new BugEntry (parsedCommit.ID, parsedCommit.BugzillaSummary, parsedCommit.Commit.Title));
					}
				}
				else
				{
					if (parsedCommit.Confidence == ParsingConfidence.High)
						collection.ConfirmedBugs.Add (new BugEntry (parsedCommit.ID, parsedCommit.BugzillaSummary, parsedCommit.Commit.Title));
					else
						collection.UncertainBugs.Add (new BugEntry (parsedCommit.ID, parsedCommit.BugzillaSummary, parsedCommit.Commit.Title));
					handledBugs.Add (parsedCommit.ID);
				}
			}

			return collection;
		}
	}
}
