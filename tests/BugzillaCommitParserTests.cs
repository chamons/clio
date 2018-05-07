using System.Collections.Generic;
using System.Linq;

using clio.Model;
using clio.Providers.Parsers;
using NUnit.Framework;

namespace clio.Tests
{
	[TestFixture]
	public class BugzillaCommitParserTests
	{
		static List<ParsedCommit> Parse (string description) => BugzillaCommitParser.Instance.ParseSingle (new CommitInfo ("hash", "title", description)).ToList ();

		static void AssertHigh (string description, int number) => ParserTestHelpers.AssertFoundWithConfidence (ParsingConfidence.High, number, Parse (description));
		static void AssertLikely (string description, int number) => ParserTestHelpers.AssertFoundWithConfidence (ParsingConfidence.Likely, number, Parse (description));
		static void AssertLow (string description, int number) => ParserTestHelpers.AssertFoundWithConfidence (ParsingConfidence.Low, number, Parse (description));

		static void AssertNone (string description)
		{
			var results = Parse (description);
			Assert.AreEqual (0, results.Count, "found an item when it should not have");
		}

		[Test]
		public void CommitParser_FindsNonExistantBugzilla_GivesLow ()
		{
			AssertNone ("https://bugzilla.xamarin.com/show_bug.cgi?id=350001");
		}

		// This behavior is less than optimal - 20000x is counted as 20000
		[Test]
		public void CommitParser_HandlesCommitWithTypeAtEnd_ByIgnoring ()
		{
			AssertHigh ("https://bugzilla.xamarin.com/show_bug.cgi?id=20000x", 20000);
		}

		[Test]
		public void CommitParser_FindsValidBugzilla_GivesHigh ()
		{
			AssertHigh ("bugzilla 58119", 58119);
			AssertLikely ("bug 58119", 58119);
			AssertLikely ("Fix a bug that makes stuff appear\nbug 58119", 58119);
			AssertHigh ("bxc 58119", 58119);
			AssertLikely ("Fix bug with stuff\nfix 58119", 58119);
			AssertHigh ("One last time with full url\n- https://bugzilla.xamarin.com/show_bug.cgi?id=58119", 58119);
			AssertHigh ("One last time with full url\n- https://bugzilla.xamarin.com/show_bug.cgi?id=58119", 58119);
			AssertNone ("Fixes 350001");

			AssertHigh ("bugzilla #58119", 58119);
			AssertLikely ("bug #58119", 58119);
			AssertHigh ("bxc #58119", 58119);
			AssertLikely ("fix #58119", 58119);
			AssertNone ("Fix #350001");
			AssertNone ("Fixes #350001");
		}

		[Test]
		public void CommitParser_HandlesInvalidFormatted_WithInvalid ()
		{
			AssertNone ("fix123");
			AssertNone ("fix#123");
			AssertNone ("fix");
			AssertNone ("fix#");
		}

		[Test]
		public void CommitParser_HandlesNoCommitLinks ()
		{
			AssertNone ("Do awesome feature in PR");
		}

		[Test]
		public void CommitWithMultipleBugs_ShowsAllBugs ()
		{
			var commits = Parse (@"Commit message monodroid/commit/2610909caf6c79a4090f7e6d53b79da33d60047c
	Fixes: https://bugzilla.xamarin.com/show_bug.cgi?id=55561
    Fixes: https://bugzilla.xamarin.com/show_bug.cgi?id=56653
	Fixes: https://bugzilla.xamarin.com/show_bug.cgi?id=57808

	Context: https://bugzilla.xamarin.com/show_bug.cgi?id=32861
	Context: https://bugzilla.xamarin.com/show_bug.cgi?id=56581
	Context: https://bugzilla.xamarin.com/show_bug.cgi?id=57532#c5

	Improve emulator checks.

	Fixes test-generator.");

			// Should be 3 - https://github.com/chamons/clio/issues/44
			Assert.AreEqual (6, commits.Count);
			Assert.True (commits.Exists (x => x.IssueId == 55561));
			Assert.True (commits.Exists (x => x.IssueId == 56653));
			Assert.True (commits.Exists (x => x.IssueId == 57808));

			commits = Parse (@" Release notes from monodroid/3b997279
    
    Fixes: https://bugzilla.xamarin.com/show_bug.cgi?id=33052
    Fixes: https://bugzilla.xamarin.com/show_bug.cgi?id=55477
    Fixes: https://bugzilla.xamarin.com/show_bug.cgi?id=56867
    Fixes: https://bugzilla.xamarin.com/show_bug.cgi?id=56874
    Fixes: https://bugzilla.xamarin.com/show_bug.cgi?id=57027
    
    Uses Android NDK r14b.
    Bumps to LibZipSharp/master/4617436a.
    Bumps to mono/2017-04/1a227379
    Bumps to Java.Interop/master/4aa2cde3
    Bumps to cecil/master/f64903c0o
    
    Enhancements to AndroidClientHandler.
    
    Allows `make create-vsix` to set correct version info.");

			Assert.AreEqual (5, commits.Count);
			Assert.True (commits.Exists (x => x.IssueId == 33052));
			Assert.True (commits.Exists (x => x.IssueId == 55477));
			Assert.True (commits.Exists (x => x.IssueId == 56867));
			Assert.True (commits.Exists (x => x.IssueId == 56874));
			Assert.True (commits.Exists (x => x.IssueId == 57027));
		}
	}
}