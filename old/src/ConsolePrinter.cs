using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using clio.Model;

namespace clio
{
	class ConsoleBugPrinter
	{
		static Regex BugRegex = new Regex (@"#\s*(\d+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

		public List<string> Links { get; private set; } = new List<string> ();
		SearchOptions Options;
		string IssueURL => "https://github.com/" + Options.GithubLocation + "/issues/";
		string CommitURL => "https://github.com/" + Options.GithubLocation + "/commit/";

		public ConsoleBugPrinter (SearchOptions options)
		{
			Options = options;
		}

		public string Parse (BugEntry bug)
		{
			Links.Clear ();
			if (Options.GithubLocation != null)
				HarvestLinks (bug);
			return FormatOutput (bug);
		}

		void Add (string link)
		{
			if (!Links.Contains (link))
				Links.Add (link);
		}

		void HarvestHashLinks (string s)
		{
			foreach (Match m in BugRegex.Matches (s))
			{
				if (m.Success)
					Add (IssueURL + m.Groups[1]); // Dirty trick, this works for issues and PRs even though URL is /issues
			}
		}

		void HarvestLinks (BugEntry bug)
		{
			Add (CommitURL + bug.Commit.Commit.Hash);
			Add (bug.IssueInfo.IssueUrl);
			HarvestHashLinks (bug.Title);
			HarvestHashLinks (bug.SecondaryTitle);
		}

		string FormatOutput (BugEntry bug)
		{
			StringBuilder output = new StringBuilder ();
			output.AppendLine ($"* [{bug.Id}]({bug.IssueInfo.IssueUrl})");
			output.AppendLine ($"\t * {bug.Date}");
			output.AppendLine ($"\t * {bug.Title}");

			if (!String.IsNullOrEmpty (bug.SecondaryTitle))
				output.AppendLine ($"\t * {bug.SecondaryTitle}");

			// Only print service if we are using both
			if (!Options.IgnoreVsts && !Options.IgnoreGithub)
				output.AppendLine ($"\t * {bug.IssueInfo.IssueSource}");

			output.AppendLine ();
			return output.ToString ();
		}
	}

	class ConsolePrinter
	{
		ConsoleBugPrinter BugPrinter;
		SearchOptions Options;

		public ConsolePrinter (SearchOptions options)
		{
			BugPrinter = new ConsoleBugPrinter (options);
			Options = options;
		}

		public static ConsolePrinter Create (SearchOptions options) => new ConsolePrinter (options);

		public void PrintCommits (IEnumerable<CommitInfo> commits)
		{
			foreach (var commit in commits)
				Console.WriteLine ($"{commit.Hash} {commit.Title}");
		}

		public void PrintBugs (BugCollection bugCollection)
		{
			if (Options.SplitEnhancementBugs)
			{
				var bugs = bugCollection.Bugs.Where (x => !x.IssueInfo.IsEnhancement);
				PrintBugList ("Bugs:", bugs);

				var potentialBugs = bugCollection.PotentialBugs.Where (x => !x.IssueInfo.IsEnhancement);
				PrintBugList ("Potential Bugs:", potentialBugs);

				var enhancements = bugCollection.Bugs.Where (x => x.IssueInfo.IsEnhancement);
				PrintBugList ("Enhancements:", enhancements);

				var potentialEnhancements = bugCollection.PotentialBugs.Where (x => x.IssueInfo.IsEnhancement);
				PrintBugList ("Potential Enhancements:", potentialEnhancements);
			}
			else
			{
				PrintBugList ("Bugs:", bugCollection.Bugs);
				PrintBugList ("Potential Bugs:", bugCollection.PotentialBugs);
			}
		}

		void PrintBugList (string title, IEnumerable<BugEntry> list)
		{
			if (list.Count () > 0)
			{
				Console.WriteLine (title);
				Console.WriteLine ();
				foreach (var bug in list)
					PrintBug (bug);
			}
		}

		static void PrintLinkList (List<string> links)
		{
			if (links.Count () > 0)
			{
				Console.WriteLine ("\t* Links");
				foreach (var link in links)
					Console.WriteLine ($"\t * [{link}]({link})");
			}
		}

		void PrintBug (BugEntry bug)
		{
			Console.WriteLine (BugPrinter.Parse (bug));

			PrintLinkList (BugPrinter.Links);

			if (Options.AdditionalBugInfo)
			{
				string additionalInfo = bug.IssueInfo.MoreInfo;
				if (additionalInfo != null)
					Console.WriteLine ($"\t{additionalInfo}");
			}
		}
	}
}
