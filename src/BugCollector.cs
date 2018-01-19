using System;
using System.Collections.Generic;
using System.Linq;
using clio.Model;

namespace clio
{
	// Collects bugs into BugCollections
	public static class BugCollector
	{
		public static BugCollection ClassifyCommits (IEnumerable<ParsedCommit> commits, SearchOptions options) => ClassifyCommits (commits, options, Enumerable.Empty<ParsedCommit> ());

		public static BugCollection ClassifyCommits (IEnumerable<ParsedCommit> commits, SearchOptions options, IEnumerable<ParsedCommit> commitsToIgnore)
		{
			// build hashsets for each issue source
			var bugsToIgnore = new Dictionary<IssueSource, HashSet<int>> ();
			var handledBugs = new Dictionary<IssueSource, HashSet<int>> ();

			Explain.Print ($"\nClassifying {commits.Count ()} commits ignoring {commitsToIgnore.Count ()} commit.");
			foreach (var source in Enum.GetValues (typeof (IssueSource)).OfType<IssueSource> ())
			{
				bugsToIgnore[source] = new HashSet<int> (commitsToIgnore.Where (x => x.IssueSource == source).Select (x => x.IssueId));
				handledBugs[source] = new HashSet<int> ();

				if (bugsToIgnore[source].Count > 0)
					Explain.Print ($"\t{source} {String.Join (" ", bugsToIgnore[source].Select (x => x.ToString ()))}");
			}

			BugCollection collection = new BugCollection ();

			// for each commit, classify the bug into one of the 2 bug lists
			// adding just 1 instance of a bug to either of the lists
			// if we see a bug a second time, only update the instance in the list
			// if the confidence is higher than what we have already

			foreach (var parsedCommit in commits.Where (x => !bugsToIgnore[x.IssueSource].Contains (x.IssueId)))
			{
				// have we handled this bug, for this issue source, before?
				if (handledBugs[parsedCommit.IssueSource].Contains (parsedCommit.IssueId))
				{
					// if so, then update if we have a higher confidence
					if (parsedCommit.Confidence == ParsingConfidence.High || parsedCommit.Confidence == ParsingConfidence.Likely)
					{
						// we should update by removing it from PotentialBugs and adding to Bugs
						if (collection.PotentialBugs.Any (x => x.IssueInfo.IssueSource == parsedCommit.IssueSource && x.Id == parsedCommit.IssueId))
						{
							collection.PotentialBugs.RemoveAll (x => x.IssueInfo.IssueSource == parsedCommit.IssueSource && x.Id == parsedCommit.IssueId);
							collection.Bugs.Add (new BugEntry (parsedCommit));
						}
					}
				}
				else
				{
					// we have not seen this issue before, lets add it to the collection

					// Low goes into PotentialBugs, otherwise (High and Likely) go into Bugs
					if (parsedCommit.Confidence == ParsingConfidence.Low)
						collection.PotentialBugs.Add (new BugEntry (parsedCommit));
					else
						collection.Bugs.Add (new BugEntry (parsedCommit));

					handledBugs[parsedCommit.IssueSource].Add (parsedCommit.IssueId);
				}
			}

			PostProcessCollection (collection);


			Explain.Print ($"\nClassified {collection.Bugs.Count ()} bug(s) and {collection.PotentialBugs.Count ()} potential bug(s).");

			return new BugCollection (collection.Bugs.OrderBy (x => x.IssueInfo.IssueSource).OrderBy (x => x.Id),
									  collection.PotentialBugs.OrderBy (x => x.IssueInfo.IssueSource).OrderBy (x => x.Id));
		}

		static void PostProcessCollection (BugCollection collection)
		{
			var bugzillaIds = new HashSet<int> (collection.Bugs.Where (x => x.IssueInfo.IssueSource == IssueSource.Bugzilla).Select (x => x.Id));
			var overlappingBugs = collection.Bugs.Where (x => x.IssueInfo.IssueSource != IssueSource.Bugzilla && bugzillaIds.Contains (x.Id)).ToList ();

			// If bugzilla and another provider both find an issue, demote the bugzilla copy to PotentialBug
			collection.Demote (overlappingBugs);
		}
	}
}
