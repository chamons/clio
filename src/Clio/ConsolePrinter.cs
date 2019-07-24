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

		public string FixLink (string link) => link.Replace ("api.github.com/repos", "github.com");

		void Add (string link)
		{
			link = FixLink (link);

			if (!Links.Contains (link))
				Links.Add (link);
		}

		void HarvestHashLinks (string s)
		{
			// It is valid to have empty descriptions, and OctoKit kindly gives us nulls ;(
			if (s == null) 
				return;
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
			HarvestHashLinks (pr.CommitInfo.Description);
			HarvestHashLinks (pr.PRInfo.Title);
			HarvestHashLinks (pr.PRInfo.Description);
		}

		string LabelSuffix (RequestInfo pr)
		{
			StringBuilder suffix = new StringBuilder ();

			if (pr.Labels.Contains ("community"))
				suffix.Append ("  *Community Contribution* ❤️");
			if (pr.Labels.Contains ("regression"))

				suffix.Append ("  *Regression* ⚠️️");
			if (pr.Labels.Contains ("note-deprecation"))
				suffix.Append ("  *Deprecation* ℹ️");
			return suffix.ToString ();
		}

		string FormatOutput (RequestInfo pr)
		{
			StringBuilder output = new StringBuilder ();
			output.AppendLine ($"* [{pr.ID}]({FixLink (pr.URL)}) - {pr.PRInfo.Title}{LabelSuffix(pr)}");

			if (!String.IsNullOrEmpty (pr.CommitInfo.Title))
				output.AppendLine ($"\t * {pr.CommitInfo.Title}");

			output.AppendLine ($"\t * {pr.Date}");

			if (pr.Labels.Count > 0)
				output.AppendLine ($"\t * {string.Join (" ", pr.Labels)}");
			return output.ToString ();
		}
	}

	class ConsolePrinter
	{
		ConsolePRPrinter Printer;
		HashSet <RequestInfo> AlreadyPrinted = new HashSet<RequestInfo> ();
		RequestCollection Requests;

		public ConsolePrinter (RequestCollection requests, string location)
		{
			Printer = new ConsolePRPrinter (location);
			Requests = requests;
		}

		public void Print (bool printAuthors)
		{
			PrintList ("", Requests.Highlights);
			PrintList ("", Requests.Breaking);
			PrintList ("Enhancements:", Requests.Enhancements);
			PrintList ("Bugs:", Requests.Bugs);
			
			PrintList ("All:", Requests.All);

			if (printAuthors) {
				HashSet<string> authors = new HashSet<string> (Requests.All.Select (x => x.Author));
				PrintList ("Authors:", authors); 
			}
		}

		void PrintList (string title, IEnumerable<RequestInfo> list)
		{
			var listToPrint = list.Where (x => !AlreadyPrinted.Contains (x));
			if (listToPrint.Count () > 0)
			{
				if (!string.IsNullOrEmpty (title)) {
					Console.WriteLine (title);
					Console.WriteLine ();
				}
				foreach (var pr in listToPrint) {
					AlreadyPrinted.Add (pr);
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

		static Regex IssueURLRegex = new Regex (@"^http(s)?://github\.com/.*/.*/issues/(\d*)$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

		string GetLinkContent (string link)
		{
			foreach (Match m in IssueURLRegex.Matches (link))
			{
				if (m.Success) {
					if (int.TryParse (m.Groups[2].Value, out int issueID)) {
						var issue = Requests.AllIssues.FirstOrDefault (x => x.Number == issueID);
						if (issue != null)
							return " - " + issue.Title;
					}
				}
			}
			return "";
		}

		void PrintLinkList (RequestInfo pr, List<string> links)
		{
			if (links.Count () > 0)
			{
				string titleLink = Printer.FixLink (pr.URL);
				foreach (var link in links.Where (x => x != titleLink)) {

					string linkContent = GetLinkContent (link);
					Console.WriteLine ($"\t * [{link}]({link}){linkContent}");
				}
			}
		}

		void PrintPR (RequestInfo pr)
		{
			Console.Write (Printer.Parse (pr));
			PrintLinkList (pr, Printer.Links);
		}
	}
}
