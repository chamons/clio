using System;
using System.IO;
using System.Threading.Tasks;
using CodeRinseRepeat.Bugzilla;

namespace clio
{
	public class BugzillaChecker
	{
		BugzillaClient client;

		public BugzillaChecker ()
		{
			client = new BugzillaClient (new Uri (@"https://bugzilla.xamarin.com/jsonrpc.cgi"));
		}

		static string LoginFilePath = Path.Combine (Environment.GetFolderPath (Environment.SpecialFolder.MyDocuments), ".bugzilla");

		ValueTuple<string, string> GetLogin ()
		{
			string login = Environment.GetEnvironmentVariable ("BUGZILLA_LOGIN");
			string password = Environment.GetEnvironmentVariable ("BUGZILLA_PASSWORD");

			if (login != null && password != null)
				return new ValueTuple<string, string> (login, password);

			if (File.Exists (LoginFilePath))
			{
				string [] loginText = File.ReadAllLines (LoginFilePath);
				if (loginText.Length == 2)
					return new ValueTuple<string, string> (loginText[0], loginText[1]);
			}

			throw new InvalidOperationException ("Unable to determine bugzilla login infomration. Please set BUGZILLA_LOGIN/BUGZILLA_PASSWORD environmental variable or create ~/.bugzilla with two lines");
		}

		public async Task Setup ()
		{
			var login = GetLogin ();
			await client.LoginAsync (login.Item1, login.Item2);
		}

		public async Task<string> GetTitle (int number)
		{
			try
			{
				Bug bug = await client.GetBugAsync (number);
				if (bug != null)
					return bug.Summary;
				else
					return null;
			}
			catch (AggregateException e)
			{
				return null;
			}
		}
	}
}
