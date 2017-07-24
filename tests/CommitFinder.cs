using NUnit.Framework;
using System;
using clio;
using System.Linq;

namespace clio.Tests
{
	[TestFixture]
	public class CommitFinderTests
	{
		[Test]
		public void CommitFinder_InvalidPath_ReturnsEmpty ()
		{
			var commits = CommitFinder.Parse ("/not/a/path", "master");
			Assert.AreEqual (0, commits.Count ());
		}

		[Test]
		public void CommitFinder_InvalidBranch_ReturnsEmpty ()
		{
			var commits = CommitFinder.Parse (TestDataLocator.GetPath (), "a-non-existant-branch");
			Assert.AreEqual (0, commits.Count ());
		}

		[Test]
		public void CommitFinder_Path_ReturnsEntries ()
		{
			var commits = CommitFinder.Parse (TestDataLocator.GetPath (), "master");
			int count = commits.Count ();
			Assert.AreNotEqual (0, commits.Count ());
			foreach (var commit in commits)
			{
				Assert.IsNotNull (commit.Hash);
				Assert.IsNotNull (commit.Title);
				Assert.IsNotNull (commit.Description);
			}

		}
	}
}