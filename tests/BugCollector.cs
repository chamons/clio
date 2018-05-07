using NUnit.Framework;
using System;
using clio;
using System.Linq;
using clio.Model;
using Optional;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace clio.Tests
{
	[TestFixture]
	public class BugCollectorTests
	{
		[Test]
		public async Task BugCollector_HandlesDuplicateBugEntries ()
		{
			var commits = new List<CommitInfo> () { new CommitInfo ("first", "title 1", "But this one though...\nbug 37664"),
													new CommitInfo ("second", "title 2", "Get context test right\nContext bug 37664") };

			var parsedCommits = await CommitParser.ParseAndValidateAsync (commits, new SearchOptions ());

			var bugCollection = BugCollector.ClassifyCommits (parsedCommits, new SearchOptions ());

			Assert.AreEqual (1, bugCollection.PotentialBugs.Count);
		}

		[Test]
		public async Task BugCollector_SmokeTest ()
		{
			var commits = new List<CommitInfo> () { new CommitInfo ("first", "title 1", "But this one though...\nbug 37664"),
													new CommitInfo ("second", "title 2", "Get context test right\nContext bug 37664"),
													new CommitInfo ("third", "title 3", "bug 37665") };

			var parsedCommits = await CommitParser.ParseAndValidateAsync (commits, new SearchOptions ());

			var bugCollection = BugCollector.ClassifyCommits (parsedCommits, new SearchOptions ());
			Assert.AreEqual (2, bugCollection.PotentialBugs.Count);
		}
	}
}