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
		[Test]
		public void CommitParser_FindsValidBugzilla ()
		{
			var commit = CommitFinder.ParseSingle (TestDataLocator.GetPath (), "0ff022416059f4819673a3ae2378110858f2e853");
			Assert.IsTrue (commit.HasValue);
			commit.MatchSome (c =>
			{ 
				var parsedCommit = CommitParser.ParseSingle (c);
				Assert.True (parsedCommit.HasValue);
				parsedCommit.MatchSome (pc => {
					Assert.AreEqual ("https://bugzilla.xamarin.com/show_bug.cgi?id=200001", pc.Link);
					Assert.AreEqual (ParsingConfidence.High, pc.Confidence);
				});
			});
		}

		// TODO - Add bugzilla check for valid
		//[Test]
		//public void CommitParser_HandlesInvalidBugzillaCommit ()
		//{
		//	var commit = CommitFinder.ParseSingle (TestDataLocator.GetPath (), "a0a2db269bb36ecdfbfaef1e8806296e83c203dc");
		//	Assert.True (commit.HasValue);

		//	commit.MatchSome (c => {
		//		var parsedCommit = CommitParser.ParseSingle (c);
		//		Assert.True (parsedCommit.HasValue);
		//		parsedCommit.MatchSome (pc => {
		//			Assert.AreEqual ("https://bugzilla.xamarin.com/show_bug.cgi?id=20000x", pc.Link);
		//			Assert.AreEqual (ParsingConfidence.Low, pc.Confidence);
		//		});
		//	});
		//}

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