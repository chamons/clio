using System;
using System.Net;
using System.IO;
using System.Xml.Linq;
using System.Runtime.Serialization.Json;
using System.Threading.Tasks;
using System.Text;

namespace clio.Providers
{
	public class HttpService
	{
		internal HttpService ()
		{
		}

		public static string UserAgent = "Xamarin";

		public async Task<string> PostAsync (string url, string content = null, string user = null, string pwd = null, string contentType = null, int? expectedStatus = null, string method = "POST", string accept = null)
		{
			using (var sr = await PostAsStreamAsync (url, content, user, pwd, contentType, expectedStatus, method))
			{
				return new StreamReader (sr).ReadToEnd ();
			}
		}

		public async Task<Stream> PostAsStreamAsync (string url, string content = null, string user = null, string pwd = null, string contentType = null, int? expectedStatus = null, string method = null, string accept = null)
		{
			try
			{
				HttpWebRequest req = (HttpWebRequest)WebRequest.Create (url);
				req.UserAgent = HttpService.UserAgent;
				req.SendChunked = false;
				req.KeepAlive = false;
				req.Expect = "";
				req.AllowWriteStreamBuffering = true;
				req.Method = method ?? "POST";
				req.Headers.Add ("Content-Encoding", "utf-8");
				if (accept != null)
					req.Accept = accept;
				if (contentType != null)
					req.ContentType = contentType;
				//				if (user != null)
				//					req.Credentials = new NetworkCredential (user, pwd);
				if (user != null)
				{
					string authInfo = user + ":" + pwd;
					authInfo = Convert.ToBase64String (Encoding.Default.GetBytes (authInfo));
					req.Headers["Authorization"] = "Basic " + authInfo;
				}

				if (content != null)
				{
					using (var s = await req.GetRequestStreamAsync ())
					{
						using (StreamWriter sw = new StreamWriter (s))
						{
							sw.Write (content);
						}
					}
				}
				WebResponse resp = await req.GetResponseAsync ();
				HttpWebResponse httpResp = resp as HttpWebResponse;
				if (expectedStatus != null && httpResp != null && (int)httpResp.StatusCode != expectedStatus.Value)
					throw new WebException (string.Format ("Expected status {0}, but got {1}: {2}", expectedStatus.Value, httpResp.StatusCode, httpResp.StatusDescription));

				return resp.GetResponseStream ();
			}
			catch (WebException)
			{
				//using (StreamReader sr = new StreamReader (ex.Response.GetResponseStream ()))
				//	Console.WriteLine ("resp: " + sr.ReadToEnd ());
				throw;
			}
		}

		public async Task<Stream> GetAsStreamAsync (string url, string user = null, string pwd = null)
		{
			Console.WriteLine ("GET " + url);
			HttpWebRequest req = (HttpWebRequest)WebRequest.Create (url);
			req.UserAgent = HttpService.UserAgent;
			req.Method = "GET";
			if (user != null)
				req.Credentials = new NetworkCredential (user, pwd);
			WebResponse resp = await req.GetResponseAsync ();
			return resp.GetResponseStream ();
		}
	}
}
