﻿using System;
using System.IO;
using System.Threading.Tasks;
using clio.Model;
using clio.Providers.Issues;

namespace clio.Providers.Validators
{
	public class VstsIssueValidator : BaseIssueValidator
	{
		static string LoginFilePath = Path.Combine (Environment.GetFolderPath (Environment.SpecialFolder.MyDocuments), ".vsts");

		VisualStudioService client;

		public VstsIssueValidator (SearchOptions options) : base (IssueSource.Vsts, options)
		{
			client = new VisualStudioService ();
		}

		protected override Task SetupAsync ()
		{                                                                               
			client.Login (GetLoginPat ());
			return Task.CompletedTask;
		}

		public override async Task<IIssue> GetIssueAsync (int issueId)
		{
			var bug = await client.GetBug ("devdiv", issueId);

			return new VstsIssue (issueId, bug);
		}

		string GetLoginPat ()
		{
			if (!string.IsNullOrEmpty(Options.VstsPAT)) {
				return Options.VstsPAT;
			}

			string login = Environment.GetEnvironmentVariable ("VSTS_PAT");

			if (login != null)
				return login;

			if (File.Exists (LoginFilePath))
			{
				string[] loginText = File.ReadAllLines (LoginFilePath);
				if (loginText.Length == 1)
					return loginText[0];
			}

			throw new InvalidOperationException ("Unable to determine vsts PAT infomration. Please set VSTS_PAT environmental variable, create ~/.vsts with 1 line, or pass --vsts=disable");
		}
	}
}
