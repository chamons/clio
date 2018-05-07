using System;
using System.Collections.Generic;
using System.Linq;
using clio.Model;

namespace clio
{
	// Collects bugs into BugCollections
	public static class BugCollector
	{
		class MetaBugList
		{
			Dictionary<IssueSource, BugList> BugLists = new Dictionary<IssueSource, BugList> ();

			public MetaBugList ()
			{
				foreach (var source in Enum.GetValues (typeof (IssueSource)).OfType<IssueSource> ())
					BugLists[source] = new BugList (source);
			}
		
			public void Add (ParsedCommit commit)
			{
				BugLists[commit.IssueSource].Add (new BugEntry (commit), commit.Confidence);
			}

			public BugCollection CreateCollection ()
			{
				BugCollection bugCollection = new BugCollection ();

				// Bugzilla must come last
				foreach (BugList list in BugLists.OrderBy (x => x.Key == IssueSource.Bugzilla ? "Z" : x.Key.ToString ()).Select (x => x.Value))
				{
					foreach (var bug in list.Bugs)
						bugCollection.AddBug (bug);

					foreach (var potentialBugs in list.PotentialBugs)
						bugCollection.AddPotentialBug (potentialBugs);
				}

				bugCollection.Order ();

				return bugCollection;
			}
		}
		
		class BugList
		{
			public IssueSource Source { get; private set; }
			public List<BugEntry> Bugs { get; private set; } = new List<BugEntry> ();
			public List<BugEntry> PotentialBugs { get; private set; } = new List<BugEntry> ();

			public bool AlreadyAdded (int id) => Bugs.Any (x => x.Id == id) || PotentialBugs.Any (x => x.Id == id);

			public BugList (IssueSource issueSource) 
			{
				Source = issueSource;
			}

			public void Add (BugEntry bug, ParsingConfidence confidence)
			{
				if (AlreadyAdded (bug.Id)) {
					if (PotentialBugs.Any (x => x.Id == bug.Id))
						PromotePotentialBug (bug);
				}
				else {
					switch (confidence) {
						case ParsingConfidence.High:
						case ParsingConfidence.Likely:
							Bugs.Add (bug);
							return;
						default:
							PotentialBugs.Add (bug);
							return;
					}
				}
			}

			void PromotePotentialBug (BugEntry bug)
			{
				PotentialBugs.Remove (bug);
				Bugs.Add (bug);
			}
		}

		public static BugCollection ClassifyCommits (IEnumerable<ParsedCommit> commits)
		{
			Explain.Print ($"\nClassifying {commits.Count ()} commits.");

			MetaBugList metaBugList = new MetaBugList ();

			foreach (var parsedCommit in commits)
				metaBugList.Add (parsedCommit);

			BugCollection collection = metaBugList.CreateCollection ();
			Explain.Print ($"\nClassified {collection.Bugs.Count ()} bug(s) and {collection.PotentialBugs.Count ()} potential bug(s).");
			return collection;
		}
	}
}
