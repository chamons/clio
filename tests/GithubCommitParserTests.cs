using System;
using clio.Model;
using System.Linq;
using NUnit.Framework;
using clio.Providers;
using clio.Providers.Parsers;

namespace clio.Tests
{
	[TestFixture]
	public class GithubCommitParserTests
	{
		[Test]
		public void ParseFullLink ()
		{
			AssertFound (new CommitInfo ("hash", "title", "title \nhttps://github.com/chamons/clio/issues/18"), 18);
			AssertFound (new CommitInfo ("hash", "title", "title \nhttp://github.com/chamons/clio/issues/18"), 18);
		}

		[Test]
		[TestCase ("title \nfixes 549249")]
		[TestCase ("title \nfixes github 549249")]
		[TestCase ("title \nfixes github bug 549249")]
		[TestCase ("title \nfixes issue 549249")]
		[TestCase ("title \nfixes bug 549249")]
		[TestCase ("title \nfixes bug #549249")]
		[TestCase ("title \nfixes fix 549249")]
		[TestCase ("title \nfixes fixes: 549249")]
		[TestCase ("title \nfixes #549249")]
		public void ParseNonFullLinks_ShouldFail (string description)
		{
			AssertNone (new CommitInfo ("hash", "title", description));
		}

		[Test]
		[TestCase ("title \nhttps://github.com/chamons/clio/issues/#124124")]
		[TestCase ("title \nhttps://github.com/chamons/clio/issues/asdf")]
		[TestCase ("title \nhttps://github.com/chamons/%@#@%")]
		[TestCase ("title \nhttps://github.com/chamons/")]
		public void ParseInvalidURL_Fails (string description)
		{
			AssertNone (new CommitInfo ("hash", "title", description));
		}

		[Test]
		public void ParseMultipleLinks ()
		{
			var commitInfo = new CommitInfo ("hash", "title", "title \nhttps://github.com/chamons/clio/issues/18\nhttps://github.com/chamons/clio/issues/19");

			var results = GithubCommitParser.Instance.ParseSingle (commitInfo).ToList ();

			Assert.AreEqual (2, results.Count, "did not find any github issues");
			Assert.AreEqual (18, results[0].IssueId, "did not parse to correct github id");
			Assert.AreEqual (19, results[1].IssueId, "did not parse to correct github id");
			Assert.AreEqual (ParsingConfidence.High, results[0].Confidence, "Did not determine the correct confidence");
			Assert.AreEqual (ParsingConfidence.High, results[1].Confidence, "Did not determine the correct confidence");
		}

		static void AssertFound (CommitInfo commitInfo, int number)
		{
			var results = GithubCommitParser.Instance.ParseSingle (commitInfo).ToList ();

			Assert.AreEqual (1, results.Count, "did not find any github issues");
			Assert.AreEqual (number, results[0].IssueId, "did not parse to correct github id");
			Assert.AreEqual (ParsingConfidence.High, results[0].Confidence, "Did not determine the correct confidence");
		}

		static void AssertNone (CommitInfo commitInfo)
		{
			var results = GithubCommitParser.Instance.ParseSingle (commitInfo).ToList ();

			Assert.AreEqual (0, results.Count, "found an item when it should not have");
		}
	}
}
