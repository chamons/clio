using System.Collections.Generic;
using System.Linq;

namespace clio.Model
{
	/// <summary>
	/// A collection of bugs, grouped by confidence
	/// </summary>
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

		/// <summary>
		/// Gets the list of bugs that have high or likely confidence
		/// </summary>
		public List<BugEntry> Bugs { get; private set; } = new List<BugEntry> ();

		/// <summary>
		/// Gets the list of bugs that have low confidence
		/// </summary>
		public List<BugEntry> PotentialBugs { get; private set; } = new List<BugEntry> ();

		public void Demote (IEnumerable<BugEntry> bugs)
		{
			Bugs = Bugs.Except (bugs).ToList ();
			PotentialBugs.AddRange (bugs);
		}
	}
}
