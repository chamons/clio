using NUnit.Framework;
using System;
using clio;
using System.Linq;
using clio.Model;
using Optional;

namespace clio.Tests
{
	[TestFixture]
	public class BugCollectorTests
	{
		[Test]
		public void BugCollector_HandlesDuplicateBugEntries ()
		{
			// One commit with certain, one without. Only one copy in final output
			var options = new SearchOptions { Oldest = "ad26139".Some (), Newest = "6c280ad".Some () };
			var commits = CommitFinder.Parse (TestDataLocator.GetPath (), options);
			var parsedCommits = CommitParser.Parse (commits, options).ToList ();

			var bugCollection = BugCollector.ClassifyCommits (parsedCommits, new SearchOptions ());

			Assert.AreEqual (1, bugCollection.Bugs.Count);
			Assert.AreEqual (0, bugCollection.PotentialBugs.Count);
		}

		[Test]
		public void BugCollector_SmokeTest ()
		{
			var options = new SearchOptions { Oldest = "98fff31".Some (), Newest = "6c280ad".Some () };
			var commits = CommitFinder.Parse (TestDataLocator.GetPath (), options);
			var parsedCommits = CommitParser.Parse (commits, options).ToList ();

			var bugCollection = BugCollector.ClassifyCommits (parsedCommits, new SearchOptions ());
			Assert.AreEqual (2, bugCollection.Bugs.Count);
			Assert.AreEqual (3, bugCollection.PotentialBugs.Count);
		}	
	}
}