using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

using Clio.Requests;

namespace Clio
{
	class ConsolePRPrinter
	{
		static Regex BugRegex = new Regex (@"#\s*(\d+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

		string Location;
		public List<string> Links { get; private set; } = new List<string> ();
		string IssueURL => "https://github.com/" + Location + "/issues/";
		string CommitURL => "https://github.com/" + Location + "/commit/";

		public ConsolePRPrinter (string location)
		{
			Location = location;
		}

		public string Parse (RequestInfo pr)
		{
			Links.Clear ();
			HarvestLinks (pr);
			return FormatOutput (pr);
		}

		void Add (string link)
		{
			link = link.Replace ("api.github.com/repos", "github.com");

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

		void HarvestLinks (RequestInfo pr)
		{
			Add (CommitURL + pr.Hash);
			Add (pr.URL);
			HarvestHashLinks (pr.CommitInfo.Title);
			HarvestHashLinks (pr.CommitInfo.Description);
			HarvestHashLinks (pr.PRInfo.Title);
			HarvestHashLinks (pr.PRInfo.Description);
		}

		string FormatOutput (RequestInfo pr)
		{
			StringBuilder output = new StringBuilder ();
			output.AppendLine ($"* [{pr.ID}]({pr.URL})");
			output.AppendLine ($"\t * {pr.PRInfo.Title}");

			if (!String.IsNullOrEmpty (pr.CommitInfo.Title))
				output.AppendLine ($"\t * {pr.CommitInfo.Title}");

			output.AppendLine ($"\t * {pr.Date}");
			return output.ToString ();
		}
	}

	class ConsolePrinter
	{
		ConsolePRPrinter Printer;

		public ConsolePrinter (string location)
		{
			Printer = new ConsolePRPrinter (location);
		}

		public static ConsolePrinter Create (string location) => new ConsolePrinter (location);

		public void PrintCommits (IEnumerable<RequestInfo> prs)
		{
			foreach (var pr in prs)
				Console.WriteLine ($"{pr.Hash} {pr.PRInfo.Title}");
		}

		public void Print (RequestCollection requests, bool printAuthors)
		{
			PrintList ("All:", requests.All);

			if (printAuthors) {
				HashSet<string> authors = new HashSet<string> (requests.All.Select (x => x.Author));
				PrintList ("Authors:", authors); 
			}
		}

		void PrintList (string title, IEnumerable<RequestInfo> list)
		{
			if (list.Count () > 0)
			{
				Console.WriteLine (title);
				Console.WriteLine ();
				foreach (var pr in list) {
					PrintPR (pr);
					Console.WriteLine ();
				}
			}
		}


		void PrintList (string title, IEnumerable<string> list)
		{
			if (list.Count () > 0)
			{
				Console.WriteLine (title);
				Console.WriteLine ();
				foreach (var item in list)
					Console.WriteLine (item);
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

		void PrintPR (RequestInfo pr)
		{
			Console.WriteLine (Printer.Parse (pr));
			PrintLinkList (Printer.Links);
		}
	}
}
