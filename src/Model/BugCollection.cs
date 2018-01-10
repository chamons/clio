using System.Collections.Generic;

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
			this.Bugs.AddRange (bugs);
			this.PotentialBugs.AddRange (potentialBugs);
		}

		/// <summary>
		/// Gets the list of bugs that have high or likely confidence
		/// </summary>
		public List<BugEntry> Bugs { get; } = new List<BugEntry> ();

		/// <summary>
		/// Gets the list of bugs that have low confidence
		/// </summary>
		public List<BugEntry> PotentialBugs { get; } = new List<BugEntry> ();
	}
}
