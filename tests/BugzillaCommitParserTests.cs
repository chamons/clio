using NUnit.Framework;
using System;
using clio;
using System.Linq;
using clio.Model;
using Optional;
using System.Collections.Generic;
using System.Threading.Tasks;
using clio.Providers;

namespace clio.Tests
{
    [TestFixture]
    public class BugzillaCommitParserTests
    {
        Task TestConfidenceOfCommit(string hash, string expectedUrl, ParsingConfidence expectedConfidence) => TestConfidenceOfCommit(hash, expectedUrl, expectedConfidence, Option.None<SearchOptions>());

        async Task TestConfidenceOfCommit(string hash, string expectedUrl, ParsingConfidence expectedConfidence, Option<SearchOptions> options)
        {
            var commit = CommitFinder.ParseSingle(TestDataLocator.GetPath(), hash);
            Assert.IsTrue(commit.HasValue);
            commit.MatchSome(async c =>
           {
               var parsedCommits = await BugzillaCommitParser.Instance.ParseSingle(c)
                                                       .ValidateAsync(options.ValueOr(new SearchOptions() { Bugzilla = BugzillaLevel.Private }));

               Assert.AreEqual(1, parsedCommits.Count(), $"{hash} did not parse into one bug commit");
               var parsedCommit = parsedCommits.First();

               Assert.AreEqual(expectedUrl, parsedCommit.Link, $"{hash} link {parsedCommit.Link} did not match expected {expectedUrl}");
               Assert.AreEqual(expectedConfidence, parsedCommit.Confidence, $"{hash} confidence {parsedCommit.Confidence} did not match expected {expectedConfidence}");
           });
        }

        Task TestCommitHasNoBug(string hash) => TestCommitHasNoBug(hash, Option.None<SearchOptions>());

        async Task TestCommitHasNoBug(string hash, Option<SearchOptions> options)
        {
            var commit = CommitFinder.ParseSingle(TestDataLocator.GetPath(), hash);
            Assert.True(commit.HasValue);
            commit.MatchSome(async c =>
            {
                var parsedCommits = await BugzillaCommitParser.Instance.ParseSingle(c)
                                                        .ValidateAsync(options.ValueOr(new SearchOptions()));
                Assert.Zero(parsedCommits.Count());
            });
        }

        Task TestMultipleCommits(string hash, List<string> expectedUrls) => TestMultipleCommits(hash, expectedUrls, Option.None<SearchOptions>());

        async Task TestMultipleCommits(string hash, List<string> expectedUrls, Option<SearchOptions> options)
        {
            var commit = CommitFinder.ParseSingle(TestDataLocator.GetPath(), hash);
            Assert.IsTrue(commit.HasValue);
            commit.MatchSome(async c =>
           {
               var parsedCommits = await BugzillaCommitParser.Instance.ParseSingle(c)
                                                       .ValidateAsync(options.ValueOr(new SearchOptions()));
               Assert.AreEqual(parsedCommits.Count(), expectedUrls.Count);
               Assert.True(new HashSet<string>(parsedCommits.Select(x => x.Link)).SetEquals(expectedUrls));
           });
        }

        [Test]
        public void CommitParser_FindsNonExistantBugzilla_GivesLow()
        {
            TestConfidenceOfCommit("0ff022416059f4819673a3ae2378110858f2e853", "https://bugzilla.xamarin.com/show_bug.cgi?id=200001", ParsingConfidence.Low);
        }

        // This behavior is less than optimal - 20000x is counted as 20000
        [Test]
        public void CommitParser_HandlesCommitWithTypeAtEnd_ByIgnoring()
        {
            TestConfidenceOfCommit("a0a2db269bb36ecdfbfaef1e8806296e83c203dc", "https://bugzilla.xamarin.com/show_bug.cgi?id=20000", ParsingConfidence.High);
        }

        [Test]
        public void CommitParser_FindsValidBugzilla_GivesHigh()
        {
            TestConfidenceOfCommit("4bb85fb7ca3bc3eebd378c5ee3a500f58c89f296", "bugzilla 58119", ParsingConfidence.High);
            TestConfidenceOfCommit("7fb24dbcef6044c8601633b74b297e811c6aedd6", "bug 58119", ParsingConfidence.High);
            TestConfidenceOfCommit("ef8df8bacfbfc8734a913138438c422b88caa003", "bug 58119", ParsingConfidence.High);
            TestConfidenceOfCommit("0d94343191415f75ed5cd55d932d0ac18be188ba", "bxc 58119", ParsingConfidence.High);
            TestConfidenceOfCommit("4cde556b508f921d542ed42c94207cbae0d788c7", "fix 58119", ParsingConfidence.High);
            TestConfidenceOfCommit("dc48bf12f33cbb7c60b636774a9bc75f9868e9d5", "https://bugzilla.xamarin.com/show_bug.cgi?id=58119", ParsingConfidence.High);
            TestConfidenceOfCommit("5e77c3710fed7672c705e2823af892381da8e1de", "Fixes 200100", ParsingConfidence.Low);

            TestConfidenceOfCommit("b8c57b9f3dc91f9e90b28b29a86fd7a5d75721d2", "bugzilla #58119", ParsingConfidence.High);
            TestConfidenceOfCommit("dcd9d0ff1c3d7d5137842faa7601b1ed6edf2956", "bug #58119", ParsingConfidence.High);
            TestConfidenceOfCommit("7b6f494c5158bf4666fe3eae1278bb6c4991ec63", "bxc #58119", ParsingConfidence.High);
            TestConfidenceOfCommit("7c291a65be221810bd73a8097fe5dc2f365d21f7", "fix #58119", ParsingConfidence.High);

            TestConfidenceOfCommit("b7dc900ee6ab57337d9a5655c876cf92e2372b9e", "Fix #200100", ParsingConfidence.Low);
            TestConfidenceOfCommit("a2238572ca5385ea9c72196c87cfe9484beac2b3", "Fixes #200100", ParsingConfidence.Low);
        }

        [Test]
        public void CommitParser_HandlesInvalidFormatted_WithInvalid()
        {
            TestCommitHasNoBug("33552d61eb89ef046479798c291e3175a7834a69"); //fix123
            TestCommitHasNoBug("c214f9f9c1a968325ecb434a5ad2a3e4a2a27b06"); //fix#123
            TestCommitHasNoBug("2e70896e5da484d67dadd004188f0de3dd39a158"); //fix
            TestCommitHasNoBug("53f5dfb223100271d77d0674efbac91ff4d28c8f"); //fix#
        }

        [Test]
        public void CommitParser_HandlesNoCommitLinks()
        {
            TestCommitHasNoBug("148b7c4bcddf6ca0831fea3ad536042e9d1e349a");
        }

        [Test]
        public async Task CommitParser_SmokeTestAllCommits()
        {
            var commits = CommitFinder.Parse(TestDataLocator.GetPath(), new SearchRange());
            var parsedCommits = await CommitParser.ParseAndValidateAsync(commits, new SearchOptions());

            Assert.NotZero(commits.Count());
            foreach (var parsedCommit in parsedCommits)
            {
                Assert.IsNotNull(parsedCommit.Commit.Title);
                Assert.IsNotNull(parsedCommit.Link);
            }
        }

        [Test]
        public async Task LowNumberBugs_NeverShowUp()
        {
            SearchOptions options = new SearchOptions() { Bugzilla = BugzillaLevel.Private };
            await TestCommitHasNoBug("261dab610e5f29c77877c68ff8abe7852bf617e4", options.Some());
        }

        [Test]
        public void CommitWithMultipleBugs_ShowsAllBugs()
        {
            TestMultipleCommits("cc3924fcc86fdf30a34b58d52f4dc124c6117a8b",
                                 new List<string> { "https://bugzilla.xamarin.com/show_bug.cgi?id=57808", "https://bugzilla.xamarin.com/show_bug.cgi?id=56653", "https://bugzilla.xamarin.com/show_bug.cgi?id=55561" });

            TestMultipleCommits("f47157f09c065ef504c9c5278a4aedd2b9570ddc",
                                 new List<string> { "https://bugzilla.xamarin.com/show_bug.cgi?id=33052", "https://bugzilla.xamarin.com/show_bug.cgi?id=55477", "https://bugzilla.xamarin.com/show_bug.cgi?id=56867",
                                "https://bugzilla.xamarin.com/show_bug.cgi?id=56874", "https://bugzilla.xamarin.com/show_bug.cgi?id=57027"});

        }
    }
}