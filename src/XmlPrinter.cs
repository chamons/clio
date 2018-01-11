using System;
using System.Collections.Generic;
using System.Xml.Linq;
using clio.Model;

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
		public static void ExportBugs (IEnumerable<ParsedCommit> parsedCommits, SearchOptions options, string outputFile)
		{
			XDocument xml = new XDocument ();
			var root = new XElement ("Bugs");
			xml.Add (root);

			foreach (var commit in parsedCommits) {
				var bugElement = new XElement ("Bug");
				root.Add (bugElement);

				bugElement.Add (new XElement ("IssueSource", commit.IssueSource));
				bugElement.Add (new XElement ("Id", commit.IssueId));
				bugElement.Add (new XElement ("Confidence", commit.Confidence));
				bugElement.Add (new XElement ("Title", commit.Issue.Title));
				bugElement.Add (new XElement ("Link", commit.Link));

				// add the commit info
				var commitElement = new XElement ("Commit");
				bugElement.Add (commitElement);
				commitElement.Add (new XElement ("Hash", commit.Commit.Hash));
				commitElement.Add (new XElement ("Title", commit.Commit.Title));
				commitElement.Add (new XElement ("Description", commit.Commit.Description));
			}

			xml.Save (outputFile);
		}
	}
}
