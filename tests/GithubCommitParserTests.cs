using System;
using clio.Model;
using System.Linq;
using NUnit.Framework;
using clio.Providers;
using clio.Providers.Parsers;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace clio.Tests
{
	[TestFixture]
	public class GithubCommitParserTests
	{
		[Test]
		public void ParseFullLink ()
		{
			AssertHigh (new CommitInfo ("hash", "title", "title \nhttps://github.com/chamons/clio/issues/18"), 18);
			AssertHigh (new CommitInfo ("hash", "title", "title \nhttp://github.com/chamons/clio/issues/18"), 18);
		}

		[Test]
		[TestCase ("title \nfixes 54924")]
		[TestCase ("title \nfixes github bug 54924")]
		[TestCase ("title \nfixes bug 54924")]
		[TestCase ("title \nfixes bug #54924")]
		[TestCase ("title \nfix 54924")]
		[TestCase ("title \nfixes: 54924")]
		[TestCase ("title \nfixes #54924")]
		public void ParseNonFullLinks_ShouldPass (string description)
		{
			AssertLikely (new CommitInfo ("hash", "title", description), 54924);
		}

		[TestCase ("title \ngithub 54924")]
		[TestCase ("title \nissue 54924")]
		public void ParseUnsupportedNamed_ShouldFail (string description)
		{
			AssertNone (new CommitInfo ("hash", "title", description));
		}

		[TestCase ("title \nfixes issue 54924")]
		[TestCase ("title \nfixes github 54924")]
		[TestCase ("title \nfixes fix 54924")]
		[TestCase ("title \nfixes fixes: 54924")]
		public void ParseConfusingLinks_ShouldFail (string description)
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
		public void ParseMarkDownLink ()
		{
			AssertHigh (new CommitInfo ("hash", "title", "Fix for issue[4110] (https://github.com/xamarin/xamarin-macios/issues/4110): Details"), 4110);
		}

		[Test]
		public void ParseMultipleLinks ()
		{
			var commitInfo = new CommitInfo ("hash", "title", "title \nhttps://github.com/chamons/clio/issues/18\nhttps://github.com/chamons/clio/issues/19");

			var results = GithubCommitParser.DefaultInstance.ParseSingle (commitInfo).ToList ();

			Assert.AreEqual (2, results.Count, "did not find any github issues");
			Assert.AreEqual (18, results[0].IssueId, "did not parse to correct github id");
			Assert.AreEqual (19, results[1].IssueId, "did not parse to correct github id");
			Assert.AreEqual (ParsingConfidence.High, results[0].Confidence, "Did not determine the correct confidence #1");
			Assert.AreEqual (ParsingConfidence.High, results[1].Confidence, "Did not determine the correct confidence #2");
		}

		[Test]
		public void IssueWithBuildPath ()
		{
			var commitInfo = new CommitInfo ("hash", "title", "title \nobj/iPhone/Debug64-today-extension/Xamarin.iOS,Version=v1.0.AssemblyAttribute.fs");

			var results = GithubCommitParser.DefaultInstance.ParseSingle (commitInfo).ToList ();

			Assert.AreEqual (0, results.Count, "Found issue unnecessarily");
		}

		[Test]
		public void ParseCrossRepo ()
		{
			var commitInfo = new CommitInfo ("hash", "title", "title \nhttps://github.com/chamons/clio/issues/18\nhttps://github.com/xamarin/maccore/issues/592");

			var results = GithubCommitParser.Create("clio").ParseSingle (commitInfo).ToList ();

			Assert.AreEqual (1, results.Count, "did not find any github issues");
			Assert.AreEqual (18, results[0].IssueId, "did not parse to correct github id");

			Assert.AreEqual (ParsingConfidence.High, results[0].Confidence, "Did not determine the correct confidence #1");
		}

		[Test]
		public void IgnoreOnlyRequestedTags ()
		{
			var commits = new List<CommitInfo> { new CommitInfo ("hash1", "Fix thing", "Fix #1"),
												 new CommitInfo ("hash2", "[tests]Fix test", "Fix #2"),
												 new CommitInfo ("hash3", "Fix tests without tag", "Fix #3"),
												 new CommitInfo ("hash4", "Fix other thing", "[tests] Fix #4")
			};

			var parser = GithubCommitParser.DefaultInstance;
			var parsedCommits = commits.SelectMany (x => parser.ParseSingle (x));

			Assert.AreEqual (3, parsedCommits.Count ());
			Assert.False (parsedCommits.Any (x => x.Commit.Hash == "hash2"));
		}

		static void AssertHigh (CommitInfo commitInfo, int number)
		{
			var results = GithubCommitParser.DefaultInstance.ParseSingle (commitInfo).ToList ();
			ParserTestHelpers.AssertFoundWithConfidence (ParsingConfidence.High, number, results);
		}

		static void AssertLikely (CommitInfo commitInfo, int number)
		{
			var results = GithubCommitParser.DefaultInstance.ParseSingle (commitInfo).ToList ();
			ParserTestHelpers.AssertFoundWithConfidence (ParsingConfidence.Likely, number, results);
		}

		static void AssertNone (CommitInfo commitInfo)
		{
			var results = GithubCommitParser.DefaultInstance.ParseSingle (commitInfo).ToList ();

			Assert.AreEqual (0, results.Count, "found an item when it should not have");
		}
	}
}
