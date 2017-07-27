using System.Linq;
using NUnit.Framework;
using Optional;

namespace clio.Tests
{
	[TestFixture]
	public class CommitFinderTests
	{
		[Test]
		public void CommitFinder_ParseInvalidPath_ReturnsEmpty ()
		{
			var commits = CommitFinder.Parse ("/not/a/path", new SearchOptions ());
			Assert.Zero (commits.Count ());
		}

		[Test]
		public void CommitFinder_ParseInvalidBranch_ReturnsEmpty ()
		{
			var commits = CommitFinder.Parse (TestDataLocator.GetPath (), new SearchOptions () { Oldest = "a-non-existant-branch".Some () });
			Assert.Zero (commits.Count ());
		}

		[Test]
		public void CommitFinder_Parse_ReturnsEntries ()
		{
			var commits = CommitFinder.Parse (TestDataLocator.GetPath (), new SearchOptions ());
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
			SearchOptions options = new SearchOptions ();
			options.Oldest = "4bb85fb".Some ();
			options.Newest = "261dab6".Some ();
			var commits = CommitFinder.Parse (TestDataLocator.GetPath (), options);
			Assert.AreEqual (6, commits.Count ());

			options.IncludeOldest = false;
			commits = CommitFinder.Parse (TestDataLocator.GetPath (), options);
			Assert.AreEqual (5, commits.Count ());
		}

		[Test]
		public void CommitFinder_EndingOnlyRange_ReturnsCorrectEntires ()
		{
			SearchOptions options = new SearchOptions ();
			options.Newest = "261dab6".Some ();
			var commits = CommitFinder.Parse (TestDataLocator.GetPath (), options);
			Assert.AreEqual (20, commits.Count ());
		}

		[Test]
		public void CommitFinder_StartingOnlyRange_ReturnsCorrectEntires ()
		{
			// This is brittle if we add more tests data
			SearchOptions options = new SearchOptions ();
			options.Oldest = "261dab6".Some ();
			var commits = CommitFinder.Parse (TestDataLocator.GetPath (), options);
			Assert.AreEqual (5, commits.Count ());

			options.IncludeOldest = false;
			commits = CommitFinder.Parse (TestDataLocator.GetPath (), options);
			Assert.AreEqual (4, commits.Count ());
		}
	}
}