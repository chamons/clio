using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Optional;
using Optional.Unsafe;

namespace clio.Tests
{
	[TestFixture]
	public class CommitFinderTests
	{
		[Test]
		public void CommitFinder_ParseInvalidPath_ReturnsEmpty ()
		{
			var commits = CommitFinder.Parse ("/not/a/path", new SearchRange ());
			Assert.Zero (commits.Count ());
		}

		[Test]
		public void CommitFinder_ParseInvalidBranch_ReturnsEmpty ()
		{
			var commits = CommitFinder.Parse (TestDataLocator.GetPath (), new SearchRange () { Oldest = "a-non-existant-branch".Some () });
			Assert.Zero (commits.Count ());
		}

		[Test]
		public void CommitFinder_Parse_ReturnsEntries ()
		{
			var commits = CommitFinder.Parse (TestDataLocator.GetPath (), new SearchRange ());
			int count = commits.Count ();
			Assert.NotZero (commits.Count ());
			foreach (var commit in commits)
			{
				Assert.IsNotNull (commit.Hash);
				Assert.IsNotNull (commit.Title);
				Assert.IsNotNull (commit.Description);
			}
		}

		[Test]
		public void CommitFinder_ParseSingle_ReturnsEntry ()
		{
			var commit = CommitFinder.ParseSingle (TestDataLocator.GetPath (), "98fff3172956031c4443574d0f6c9d1e204893ae");
			Assert.IsTrue (commit.HasValue);
		}

		[Test]
		public void CommitFinder_ParseInvalidHash_ReturnsEmpty ()
		{
			var commit = CommitFinder.ParseSingle (TestDataLocator.GetPath (), "NOT_A_HASH");
			Assert.IsFalse (commit.HasValue);
		}

		[Test]
		public void CommitFinder_SubsetRange_ReturnsCorrectEntires ()
		{
			var range = new SearchRange ();
			range.Oldest = "4bb85fb".Some ();
			range.Newest = "261dab6".Some ();
			var commits = CommitFinder.Parse (TestDataLocator.GetPath (), range);
			Assert.AreEqual (6, commits.Count ());

			range.IncludeOldest = false;
			commits = CommitFinder.Parse (TestDataLocator.GetPath (), range);
			Assert.AreEqual (5, commits.Count ());
		}

		[Test]
		public void CommitFinder_EndingOnlyRange_ReturnsCorrectEntires ()
		{
			var range = new SearchRange ();
			range.Newest = "261dab6".Some ();
			var commits = CommitFinder.Parse (TestDataLocator.GetPath (), range);
			Assert.AreEqual (20, commits.Count ());
		}

		[Test]
		public void CommitFinder_StartingOnlyRange_ReturnsCorrectEntires ()
		{
			// This is brittle if we add more tests data
			var range = new SearchRange ();
			range.Oldest = "261dab6".Some ();
			var commits = CommitFinder.Parse (TestDataLocator.GetPath (), range);
			Assert.AreEqual (27, commits.Count ());

			range.IncludeOldest = false;
			commits = CommitFinder.Parse (TestDataLocator.GetPath (), range);
			Assert.AreEqual (26, commits.Count ());
		}

		[Test]
		public void CommitFinder_FindMergeBase_SmokeTest ()
		{
			var mergeBase = CommitFinder.FindMergeBase (TestDataLocator.GetPath (), "release");
			Assert.AreEqual ("3e8a6ddbef973d021ed13a48a9af2c306beef558", mergeBase.ValueOrFailure ());
		}

		[Test]
		public void CommitFinder_FindCommitOnBranchToIgnore_SmokeTest ()
		{
			var commitInfo = CommitFinder.FindCommitsOnBranchToIgnore (TestDataLocator.GetPath (), "release", new SearchOptions ());
			Assert.AreEqual ("3e8a6ddbef973d021ed13a48a9af2c306beef558", commitInfo.Item2);
			Assert.AreEqual (1, commitInfo.Item1.Count ());
			Assert.AreEqual ("7dfe9984e660d26829bbcbd29648348de9623221", commitInfo.Item1.First().Hash);
		}
	}
}