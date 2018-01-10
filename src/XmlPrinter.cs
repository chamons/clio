using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using clio.Model;
using clio.Providers;

namespace clio
{
	/// <summary>
	/// Prints information to an xml file
	/// </summary>
	static class XmlPrinter
	{
		/// <summary>
		/// Exports the given bug collection to an xml file
		/// </summary>
		public static void ExportBugs (BugCollection bugCollection, SearchOptions options, string outputFile)
		{
			XDocument xml = new XDocument ();
			var root = new XElement ("Bugs");
			xml.Add (root);

			foreach (var bug in bugCollection.Bugs) {
				var b = new XElement ("Bug");
				root.Add (b);

				b.Add (new XElement ("IssueSource", bug.IssueInfo.IssueSource));
				b.Add (new XElement ("Id", bug.Id));
				b.Add (new XElement ("Title", bug.Title));
				// TODO: complete this...
			}

			xml.Save (outputFile);




			//if (options.SplitEnhancementBugs)
			//{
			//	var bugs = bugCollection.Bugs.Where (x => !x.IssueInfo.IsEnhancement);
			//	PrintBugList ("Bugs:", false, bugs, options);

			//	var potentialBugs = bugCollection.PotentialBugs.Where (x => !x.IssueInfo.IsEnhancement);
			//	PrintBugList ("Potential Bugs:", true, potentialBugs, options);

			//	var enhancements = bugCollection.Bugs.Where (x => x.IssueInfo.IsEnhancement);
			//	PrintBugList ("Enhancements:", false, enhancements, options);

			//	var potentialEnhancements = bugCollection.PotentialBugs.Where (x => x.IssueInfo.IsEnhancement);
			//	PrintBugList ("Potential Enhancements:", true, potentialEnhancements, options);
			//}
			//else
			//{
			//	PrintBugList ("Bugs:", false, bugCollection.Bugs, options);
			//	PrintBugList ("Potential Bugs:", true, bugCollection.PotentialBugs, options);
			//}
		}

		static void PrintBugList (string title, bool potential, IEnumerable<BugEntry> list, SearchOptions options)
		{
			if (list.Count () > 0)
			{
				Console.WriteLine (title);
				foreach (var bug in list)
					PrintBug (bug, potential, options);
			}
		}

		static string FormatBug (BugEntry bug)
		{
			// If bugzilla validation is disabled, all bugs are uncertain
			if (string.IsNullOrEmpty (bug.Title))
				return FormatUncertainBug (bug);

			return $"* [{bug.Id}]({bug.IssueInfo.IssueUrl}) -  {bug.Title}" + (String.IsNullOrEmpty (bug.SecondaryTitle) ? "" : $" / {bug.SecondaryTitle}");
		}

		static string FormatUncertainBug (BugEntry bug)
		{
			return $"* [{bug.Id}]({bug.IssueInfo.IssueUrl}) -  {bug.SecondaryTitle}";
		}

		static void PrintBug (BugEntry bug, bool potential, SearchOptions options)
		{
			if (!potential)
				Console.WriteLine (FormatBug (bug));
			else
				Console.WriteLine (FormatUncertainBug (bug));

			if (options.AdditionalBugInfo)
			{
				var checker = new BugzillaIssueValidator (options);
				string additionalInfo = checker.GetIssueAsync ((int)bug.Id).Result.MoreInfo;
				if (additionalInfo != null)
					Console.WriteLine ($"\t{additionalInfo}");
			}
		}
	}
}
