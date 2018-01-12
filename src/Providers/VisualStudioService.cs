using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace clio.Providers
{
	public class VisualStudioService
	{
		string accessToken;

		public VisualStudioService ()
		{
		}

		public void Login (string pat)
		{
			accessToken = pat;
		}

		static string EncodeForUrl (string s)
		{
			return s.Replace (" ", "%20");
		}

		public async Task<VisualStudioBug> GetBug (string account, int bugId)
		{
			try
			{
				try
				{
					var httpService = new HttpService ();

					var url = string.Format ("https://{0}.visualstudio.com/_apis/wit/workitems/{1}?$expand=all&api-version=2.2", EncodeForUrl (account), bugId);
					var res = await httpService.PostAsync (url, user: "", pwd: accessToken, contentType: "application/json-patch+json", method: "GET");
					return (VisualStudioBug)JsonConvert.DeserializeObject (res, typeof (VisualStudioBug));
				}
				catch (WebException ex)
				{
					ParseError (ex);
					throw;
				}
			}
			catch
			{
				// TODO: log errors somehow
				return null;
			}
		}

		public async Task<VisualStudioComment[]> GetBugComments (string account, int bugId)
		{
			var url = string.Format ("https://{0}.visualstudio.com/_apis/wit/workitems/{1}/revisions?fields=System.History&api-version=2.2", EncodeForUrl (account), bugId);
			try
			{
				var httpService = new HttpService ();

				var res = await httpService.PostAsync (url, user: "", pwd: accessToken, contentType: "application/json-patch+json", method: "GET");
				var revs = (VisualStudioRevisions)JsonConvert.DeserializeObject (res, typeof (VisualStudioRevisions));
				List<VisualStudioComment> comments = new List<VisualStudioComment> ();
				string lastComment = null;
				foreach (var bug in revs.Value)
				{
					if (!bug.Fields.TryGetValue ("System.History", out var text))
						continue;
					if (text != lastComment)
					{
						lastComment = text;
						var date = DateTime.Parse (bug.Fields["System.ChangedDate"]);
						comments.Add (new VisualStudioComment
						{
							Revision = bug.Rev,
							ChangeDate = date,
							ChangedBy = bug.Fields["System.ChangedBy"],
							Text = text
						});
					}
				}
				return comments.ToArray ();
			}
			catch (WebException ex)
			{
				ParseError (ex);
				throw;
			}
		}

		void ParseError (WebException ex)
		{
			if (ex.Response != null)
			{
				using (StreamReader sr = new StreamReader (ex.Response.GetResponseStream ()))
				{
					VisualStudioError error;
					try
					{
						var json = sr.ReadToEnd ();
						error = (VisualStudioError)JsonConvert.DeserializeObject (json, typeof (VisualStudioError));
					}
					catch
					{
						return;
					}
					throw new Exception (error.Message);
				}
			}
		}
	}

	class VisualStudioError
	{
		[JsonProperty ("message")]
		public string Message { get; set; }
	}

	public class VisualStudioBug
	{
		[JsonProperty ("id")]
		public string Id { get; set; }

		[JsonProperty ("rev")]
		public int Rev { get; set; }

		[JsonProperty ("fields")]
		public Dictionary<string, string> Fields { get; set; }

		[JsonProperty ("url")]
		public string Url { get; set; }
	}

	public class VisualStudioComment
	{
		public int Revision { get; set; }
		public DateTime ChangeDate { get; set; }
		public string ChangedBy { get; set; }
		public string Text { get; set; }
	}

	class VisualStudioRevisions
	{
		[JsonProperty ("count")]
		public int Count { get; set; }

		[JsonProperty ("value")]
		public VisualStudioBug[] Value { get; set; }
	}

	public class VisualStudioAreaPath
	{
		[JsonProperty ("name")]
		public string Name { get; set; }

		[JsonProperty ("children")]
		public VisualStudioAreaPath[] Children { get; set; }
	}
}