using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using clio.Model;
using CodeRinseRepeat.Bugzilla;

namespace clio.Providers.Validators
{
	/// <summary>
	/// Validates bugzilla bug entries
	/// </summary>
	public class BugzillaIssueValidator : BaseIssueValidator
	{
		static string LoginFilePath = Path.Combine (Environment.GetFolderPath (Environment.SpecialFolder.MyDocuments), ".bugzilla");

		Dictionary<int, Bug> BugCache = new Dictionary<int, Bug> ();
		BugzillaClient Client;
		bool signedIn;

		public BugzillaIssueValidator (SearchOptions options) : base (IssueSource.Bugzilla, options)
		{
			Client = new BugzillaClient (new Uri (@"https://bugzilla.xamarin.com/jsonrpc.cgi"));
		}

		public override async Task<IIssue> GetIssueAsync (int issueId)
		{
			if (!signedIn)
			{
				await this.SetupAsync ().ConfigureAwait (false);
			}

			Bug bug = await GetBug (issueId).ConfigureAwait (false);
			if (bug != null)
			{
				return new BugzillaIssue (bug);
			}

			return null;
		}

		protected override async Task SetupAsync ()
		{
			if (Options.Bugzilla != BugzillaLevel.Private)
				return;

			if (signedIn)
				return;

			var login = GetLogin ();
			await Client.LoginAsync (login.Item1, login.Item2).ConfigureAwait (false);
			signedIn = true;
		}

		ValueTuple<string, string> GetLogin ()
		{
			string login = Environment.GetEnvironmentVariable ("BUGZILLA_LOGIN");
			string password = Environment.GetEnvironmentVariable ("BUGZILLA_PASSWORD");

			if (login != null && password != null)
				return new ValueTuple<string, string> (login, password);

			if (File.Exists (LoginFilePath))
			{
				string[] loginText = File.ReadAllLines (LoginFilePath);
				if (loginText.Length == 2)
					return new ValueTuple<string, string> (loginText[0], loginText[1]);
			}

			throw new InvalidOperationException ("Unable to determine bugzilla login information. Please set BUGZILLA_LOGIN/BUGZILLA_PASSWORD environmental variable, create ~/.bugzilla with two lines, or pass --disable-bugzilla");
		}

		async Task<Bug> GetBug (int number)
		{
			Bug result;
			if (!BugCache.TryGetValue (number, out result))
			{
				try
				{
					result = await Client.GetBugAsync (number);
				}
				catch (AggregateException)
				{
					result = null;
				}
				BugCache[number] = result;
			}
			return result;
		}
	}
}
