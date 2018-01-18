using System;
using System.Collections.Generic;
using System.Linq;
using clio.Model;

namespace clio
{
	/// <summary>
	/// Validates bug milestones and statuses
	/// </summary>
	public static class BugValidator
	{
		/// <summary>
		/// Validates the bugs in the bug collection and emits information to the console
		/// </summary>
		public static void Validate (BugCollection bugs, SearchOptions options)
		{
			bool explainStatus = Explain.Enabled;
			Explain.Enabled = true;
			
			Explain.Print ("Validating Bug Status:");
			if (options.Bugzilla != BugzillaLevel.Private)
				Explain.Print ("This will only cover public bugs as private as --bugzilla:private is not set.");

			Explain.Indent ();

			ProcessBugStatus (bugs);
			Explain.Print ("");
			ProcessTargetMilestones (bugs, options);

			Explain.Deindent ();
			Explain.Enabled = explainStatus;
		}

		static void ProcessBugStatus (BugCollection bugs)
		{
			foreach (var bug in bugs.Bugs)
			{
				if (!bug.IssueInfo.IsClosed) {
					Explain.Print ($"{bug.IssueInfo.IssueSource} {bug.Id} status may not be set correctly: {bug.IssueInfo.Status}.");
				}
			}
		}

		static void ProcessTargetMilestones (BugCollection bugs, SearchOptions options)
		{
			foreach (var source in Enum.GetValues (typeof (IssueSource)).OfType<IssueSource> ())
			{
				var sourceBugs = new BugCollection (bugs.Bugs.Where (x => x.IssueInfo.IssueSource == source),
				                                    bugs.PotentialBugs.Where (x => x.IssueInfo.IssueSource == source));
				
				ProcessTargetMilestones (sourceBugs, options.ExpectedTargetMilestone);
			}
		}

		static void ProcessTargetMilestones (BugCollection bugs, string expectedTargetMilestone)
		{
			string targetMilestone = expectedTargetMilestone ?? GuessTargetMilestone (bugs);

			var unmatchingBugs = bugs.Bugs.Where (x => x.IssueInfo.TargetMilestone != targetMilestone);
			if (unmatchingBugs.Any ())
			{
				Explain.Print ($"The following bugs do not match the expected {targetMilestone}:");
				Explain.Indent ();

				foreach (var bug in unmatchingBugs)
					Explain.Print ($"{bug.IssueInfo.IssueSource} {bug.Id} - {bug.IssueInfo.TargetMilestone}");
			}

			// TODO: is this an unmatched Deindent??
			Explain.Deindent ();
		}

		static string GuessTargetMilestone (BugCollection bugs)
		{
			Explain.Print ("--expected-target-milestone was not set, so finding the most common Target Milestone.");
			var targetMilestoneCount = new Dictionary<string, int> ();
			foreach (var bug in bugs.Bugs)
			{
				if (targetMilestoneCount.ContainsKey (bug.IssueInfo.TargetMilestone))
					targetMilestoneCount[bug.IssueInfo.TargetMilestone] += 1;
				else
					targetMilestoneCount[bug.IssueInfo.TargetMilestone] = 1;
			}
			var targetMilestones = targetMilestoneCount.Keys.OrderByDescending (x => targetMilestoneCount[x]).ToList ();

			string guess = targetMilestones.FirstOrDefault ();
			Explain.Print ($"{guess} is the most common Target Milestone.");
			return guess;
		}
	}
}
