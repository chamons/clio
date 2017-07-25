using NUnit.Framework;
using System;
using clio;
using System.Linq;
using clio.Model;

namespace clio.Tests
{
	[TestFixture]
	public class CommitParserTests
	{
		void TestConfidenceOfCommit (string hash, string expectedUrl, ParsingConfidence expectedConfidence)
		{
			var commit = CommitFinder.ParseSingle (TestDataLocator.GetPath (), hash);
			Assert.IsTrue (commit.HasValue);
			commit.MatchSome (c =>
			{
				var parsedCommit = CommitParser.ParseSingle (c);
				Assert.True (parsedCommit.HasValue, $"{hash} did not parse into a bug commit");
				parsedCommit.MatchSome (pc => {
					Assert.AreEqual (expectedUrl, pc.Link, $"{hash} link {pc.Link} did not match expected {expectedUrl}");
					Assert.AreEqual (expectedConfidence, pc.Confidence, $"{hash} confidence {pc.Confidence} did not match expected {expectedConfidence}");
				});
			});
		}
		
		[Test]
		public void CommitParser_FindsNonExistantBugzilla_GivesLow ()
		{
			TestConfidenceOfCommit ("0ff022416059f4819673a3ae2378110858f2e853", "https://bugzilla.xamarin.com/show_bug.cgi?id=200001", ParsingConfidence.Low);
		}

		// This behavior is less than optimal
		[Test]
		public void CommitParser_HandlesCommitWithTypeAtEnd_ByIgnoring ()
		{
			TestConfidenceOfCommit ("a0a2db269bb36ecdfbfaef1e8806296e83c203dc", "https://bugzilla.xamarin.com/show_bug.cgi?id=20000", ParsingConfidence.Low);
		}

		[Test]
		public void CommitParser_FindsValidBugzilla_GivesHigh ()
		{
			TestConfidenceOfCommit ("4bb85fb7ca3bc3eebd378c5ee3a500f58c89f296", "bugzilla 58119", ParsingConfidence.High);
			TestConfidenceOfCommit ("7fb24dbcef6044c8601633b74b297e811c6aedd6", "bug 58119", ParsingConfidence.High);
			TestConfidenceOfCommit ("ef8df8bacfbfc8734a913138438c422b88caa003", "bug 58119", ParsingConfidence.High);
			TestConfidenceOfCommit ("0d94343191415f75ed5cd55d932d0ac18be188ba", "bxc 58119", ParsingConfidence.High);
			TestConfidenceOfCommit ("4cde556b508f921d542ed42c94207cbae0d788c7", "fix 58119", ParsingConfidence.High);
			TestConfidenceOfCommit ("dc48bf12f33cbb7c60b636774a9bc75f9868e9d5", "https://bugzilla.xamarin.com/show_bug.cgi?id=58119", ParsingConfidence.High);
		}

		[Test]
		public void CommitParser_HandlesNoCommitLinks ()
		{
			var commit = CommitFinder.ParseSingle (TestDataLocator.GetPath (), "148b7c4bcddf6ca0831fea3ad536042e9d1e349a");
			Assert.True (commit.HasValue);
			commit.MatchSome (c => {
				var parsedCommit = CommitParser.ParseSingle (c);
				Assert.False (parsedCommit.HasValue);
			});
		}

		[Test]
		public void CommitParser_SmokeTestAllCommits ()
		{
			var commits = CommitFinder.Parse (TestDataLocator.GetPath (), "master");
			var parsedCommits = CommitParser.Parse (commits);
			Assert.NotZero (commits.Count ());
			foreach (var parsedCommit in parsedCommits)
			{
				Assert.IsNotNull (parsedCommit.Commit.Title);
				Assert.IsNotNull (parsedCommit.Link);
			}
		}
	}
}