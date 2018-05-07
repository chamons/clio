using System.Collections.Generic;
using System.Linq;

namespace clio.Model
{
	public class BugCollection
	{
		public BugCollection ()
		{
		}

		public BugCollection (IEnumerable<BugEntry> bugs, IEnumerable<BugEntry> potentialBugs)
		{
			Bugs.AddRange (bugs);
			PotentialBugs.AddRange (potentialBugs);
		}

		public List<BugEntry> Bugs { get; private set; } = new List<BugEntry> ();
		public List<BugEntry> PotentialBugs { get; private set; } = new List<BugEntry> ();

		public bool Contains (int id) => ContainsBug (id) || ContainsPotentialBug (id);
		public bool ContainsBug (int id) => Bugs.Any (x => x.Id == id);
		public bool ContainsPotentialBug (int id) => PotentialBugs.Any (x => x.Id == id);

		void RemoveAnyMatchingPotentialBug (BugEntry bug)
		{
			if (PotentialBugs.Any (x => x.Id == bug.Id))
				PotentialBugs.RemoveAll (x => x.Id == bug.Id);
		}

		public void AddBug (BugEntry bug)
		{
			RemoveAnyMatchingPotentialBug (bug);
			if (!ContainsBug (bug.Id))
				Bugs.Add (bug);
		}

		public void AddPotentialBug (BugEntry bug)
		{
			if (!ContainsBug (bug.Id) && !ContainsPotentialBug (bug.Id))
				PotentialBugs.Add (bug);
		}

		public void Order ()
		{
			// Bugzilla goes last
			Bugs = Bugs.OrderBy (x => x.Id).OrderBy (x => x.IssueSource == IssueSource.Bugzilla ? "Z" : x.IssueSource.ToString ()).ToList ();
			PotentialBugs = PotentialBugs.OrderBy (x => x.Id).OrderBy (x => x.IssueSource == IssueSource.Bugzilla ? "Z" : x.IssueSource.ToString ()).ToList ();
		}
	}
}
