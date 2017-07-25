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
			var options = new SearchOptions { Starting = "ad26139".Some (), Ending = "6c280ad".Some () };
			var commits = CommitFinder.Parse (TestDataLocator.GetPath (), options);
			var parsedCommits = CommitParser.Parse (commits, options).ToList ();

			var bugCollection = BugCollector.ClassifyCommits (parsedCommits);

			Assert.AreEqual (1, bugCollection.ConfirmedBugs.Count);
			Assert.AreEqual (0, bugCollection.UncertainBugs.Count);
		}

		[Test]
		public void BugCollector_SmokeTest ()
		{
			var options = new SearchOptions { Starting = "98fff31".Some (), Ending = "6c280ad".Some () };
			var commits = CommitFinder.Parse (TestDataLocator.GetPath (), options);
			var parsedCommits = CommitParser.Parse (commits, options).ToList ();

			var bugCollection = BugCollector.ClassifyCommits (parsedCommits);
			Assert.AreEqual (2, bugCollection.ConfirmedBugs.Count);
			Assert.AreEqual (3, bugCollection.UncertainBugs.Count);
		}	
	}
}