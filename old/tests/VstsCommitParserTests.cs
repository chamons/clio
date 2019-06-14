using System;
using clio.Model;
using System.Linq;
using NUnit.Framework;
using clio.Providers;
using clio.Providers.Parsers;

namespace clio.Tests
{
	[TestFixture]
	public class VstsCommitParserTests
	{
		[Test]
		public void ParseFullLink ()
		{
			AssertHigh (new CommitInfo ("hash", "title", "title \nfixes https://devdiv.visualstudio.com/DevDiv/_workitems/edit/549249"), 549249);
		}

		[Test]
		public void ParseFixes ()
		{
			AssertLikely (new CommitInfo ("hash", "title", "title \nfixes 549249"), 549249);
		}

		[Test]
		public void ParseFixesVsts ()
		{
			AssertHigh (new CommitInfo ("hash", "title", "title \nfixes VSTS 549249"), 549249);
		}

		[Test]
		public void ParseFixesVstsBug ()
		{
			AssertHigh (new CommitInfo ("hash", "title", "title \nfixes VSTS bug 549249"), 549249);
		}

		[Test]
		public void ParseVstsBug ()
		{
			AssertHigh (new CommitInfo ("hash", "title", "title \nVSTS bug 549249"), 549249);;
		}

		[Test]
		public void ParseFixesWithColon ()
		{
			AssertLikely (new CommitInfo ("hash", "title", "title \nfixes: 549249"), 549249);
		}

		[Test]
		public void ParseFixesWithUppercase ()
		{
			AssertLikely (new CommitInfo ("hash", "title", "title \nFixes: 549249"), 549249);
		}

		[Test]
		public void ParseInvalidFixes ()
		{
			AssertNone (new CommitInfo ("hash", "title", "title \nfixes 54249"));
		}

		[Test]
		public void ParseFix ()
		{
			var commitInfo = new CommitInfo ("hash", "title", "title \nfix 549249");

			var results = VstsCommitParser.Instance.ParseSingle (commitInfo).ToList ();

			Assert.AreEqual (1, results.Count, "did not find any vsts work items");
			Assert.AreEqual (549249, results[0].IssueId, "did not parse to correct vsts id");
			Assert.AreEqual (ParsingConfidence.Likely, results[0].Confidence, "Did not determine the correct confidence");
		}

		[Test]
		public void ParseBug ()
		{
			AssertLikely (new CommitInfo ("hash", "title", "title \nbug 549249"), 549249);
		}

		[Test]
		public void ParseBugWithColon ()
		{
			AssertLikely (new CommitInfo ("hash", "title", "title \nbug: 549249"), 549249);
		}

		[Test]
		public void ParseBugWithHash1 ()
		{
			AssertLikely (new CommitInfo ("hash", "title", "title \nbug# 549249"), 549249);
		}

		[Test]
		public void ParseBugWithHash2 ()
		{
			AssertLikely (new CommitInfo ("hash", "title", "title \nbug #549249"), 549249);
		}

		[Test]
		public void ParseInvalidFix ()
		{
			AssertNone (new CommitInfo ("hash", "title", "title \nfix 54249"));
		}

		[Test]
		public void ParseMultipleLinks ()
		{
			var commitInfo = new CommitInfo ("hash", "title", "title \nfixes https://devdiv.visualstudio.com/DevDiv/_workitems/edit/549249\nfixes 300123");

			var results = VstsCommitParser.Instance.ParseSingle (commitInfo).ToList ();

			Assert.AreEqual (2, results.Count, "did not find any vsts work items");
			Assert.AreEqual (549249, results[0].IssueId, "did not parse to correct vsts id");
			Assert.AreEqual (300123, results[1].IssueId, "did not parse to correct vsts id");
			Assert.AreEqual (ParsingConfidence.High, results[0].Confidence, "Did not determine the correct confidence");
			Assert.AreEqual (ParsingConfidence.Likely, results[1].Confidence, "Did not determine the correct confidence");
		}

		static void AssertHigh (CommitInfo commitInfo, int number)
		{
			var results = VstsCommitParser.Instance.ParseSingle (commitInfo).ToList ();
			ParserTestHelpers.AssertFoundWithConfidence (ParsingConfidence.High, number, results);
		}

		static void AssertLikely (CommitInfo commitInfo, int number)
		{
			var results = VstsCommitParser.Instance.ParseSingle (commitInfo).ToList ();
			ParserTestHelpers.AssertFoundWithConfidence (ParsingConfidence.Likely, number, results);
		}

		static void AssertNone (CommitInfo commitInfo)
		{
			var results = VstsCommitParser.Instance.ParseSingle (commitInfo).ToList ();
			Assert.AreEqual (0, results.Count, "found an item when it should not have");
		}
	}
}
