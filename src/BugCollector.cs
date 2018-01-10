using System;
using System.Collections.Generic;
using System.Linq;
using clio.Model;

namespace clio
{
	public class BugCollection
	{
		public List<BugEntry> Bugs { get; set; } = new List<BugEntry> ();
		public List<BugEntry> PotentialBugs { get; set; } = new List<BugEntry> ();
	}

	public static class BugCollector
	{
		public static BugCollection ClassifyCommits (IEnumerable<ParsedCommit> commits, SearchOptions options) => ClassifyCommits (commits, options, Enumerable.Empty<ParsedCommit> ());

		public static BugCollection ClassifyCommits (IEnumerable<ParsedCommit> commits, SearchOptions options, IEnumerable<ParsedCommit> commitsToIgnore)
		{
			var bugsToIgnore = new HashSet<int> (commitsToIgnore.Select (x => x.IssueId));

			Explain.Print ($"\nClassifying {commits.Count ()} commits ignoring {commitsToIgnore.Count ()} commit.");
			Explain.Print ($"\t{String.Join (" ", bugsToIgnore.Select (x => x.ToString ()))}");

			BugCollection collection = new BugCollection ();
			var handledBugs = new HashSet<int> ();

			foreach (var parsedCommit in commits.Where (x => !bugsToIgnore.Contains (x.IssueId)))
			{
				if (handledBugs.Contains (parsedCommit.IssueId))
				{

                    // TODO: update this to handle likely bugs


					// Commits are either high or low to get here. If we're low
					// then we do not need handling. Either before was low and 
					// we are good, or it was high and we are good
					if (parsedCommit.Confidence != ParsingConfidence.High)
						continue;

					// If we were low before, remove and replace with high
					if (collection.PotentialBugs.Any (x => x.ID == parsedCommit.IssueId))
					{
						collection.PotentialBugs.RemoveAll (x => x.ID == parsedCommit.IssueId);
                        collection.Bugs.Add (new BugEntry (parsedCommit.IssueId, parsedCommit.Issue.Title, parsedCommit.Commit.Title, parsedCommit));
					}
				}
				else
				{
                    if (parsedCommit.Confidence == ParsingConfidence.High || parsedCommit.Confidence == ParsingConfidence.Likely)
                        collection.Bugs.Add (new BugEntry (parsedCommit.IssueId, parsedCommit.Issue.Title, parsedCommit.Commit.Title, parsedCommit));
					else
                        collection.PotentialBugs.Add (new BugEntry (parsedCommit.IssueId, parsedCommit.Issue.Title, parsedCommit.Commit.Title, parsedCommit));
					handledBugs.Add (parsedCommit.IssueId);
				}
			}

			collection.Bugs = collection.Bugs.OrderBy (x => x.ID).ToList ();
			collection.PotentialBugs = collection.PotentialBugs.OrderBy (x => x.ID).ToList ();

			return collection;
		}
	}
}
