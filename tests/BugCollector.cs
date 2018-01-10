using NUnit.Framework;
using System;
using clio;
using System.Linq;
using clio.Model;
using Optional;
using System.Threading.Tasks;

namespace clio.Tests
{
	[TestFixture]
	public class BugCollectorTests
	{
		[Test]
		public async Task BugCollector_HandlesDuplicateBugEntries ()
		{
			// One commit with certain, one without. Only one copy in final output
			var options = new SearchOptions () { Bugzilla = BugzillaLevel.Private };
			var range = new SearchRange { Oldest = "ad26139".Some (), Newest = "6c280ad".Some () };
			var commits = CommitFinder.Parse (TestDataLocator.GetPath (), range);
			var parsedCommits = await CommitParser.ParseAndValidateAsync (commits, options);

			var bugCollection = BugCollector.ClassifyCommits (parsedCommits, new SearchOptions ());

			Assert.AreEqual (1, bugCollection.Bugs.Count);
			Assert.AreEqual (0, bugCollection.PotentialBugs.Count);
		}

		[Test]
		public async Task BugCollector_SmokeTest ()
		{
			var options = new SearchOptions () { Bugzilla = BugzillaLevel.Private };
			var range = new SearchRange { Oldest = "98fff31".Some (), Newest = "6c280ad".Some () };
			var commits = CommitFinder.Parse (TestDataLocator.GetPath (), range);
			var parsedCommits = await CommitParser.ParseAndValidateAsync (commits, options);

			var bugCollection = BugCollector.ClassifyCommits (parsedCommits, options);
			Assert.AreEqual (2, bugCollection.Bugs.Count);
			Assert.AreEqual (3, bugCollection.PotentialBugs.Count);
		}
	}
}