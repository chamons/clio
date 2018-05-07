using NUnit.Framework;
using System;
using clio;
using System.Linq;
using clio.Model;
using Optional;
using System.Collections.Generic;
using System.Threading.Tasks;
using clio.Providers;
using clio.Providers.Parsers;

namespace clio.Tests
{
	[TestFixture]
	public static class ParserTestHelpers
	{
		public static void AssertFoundWithConfidence (ParsingConfidence confidence, int number, List<ParsedCommit> results)
		{
			Assert.AreEqual (1, results.Count, "did not find any items");
			Assert.AreEqual (number, results[0].IssueId, "did not parse to correct number");
			Assert.AreEqual (confidence, results[0].Confidence, "Did not determine the correct confidence: " + confidence);
		}
	}
}	