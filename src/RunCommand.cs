﻿using System;
using System.Diagnostics;
using System.Text;

namespace clio
{
	public class RunCommand
	{
		public static bool Verbose { get; set; }
		
		// Stolen with love and modified from https://github.com/xamarin/xamarin-macios/blob/master/tools/common/Driver.cs#L231
		public static int Run (string path, string args, string currentDirectory, string[] env = null, StringBuilder output = null, bool suppressPrintOnErrors = false)
		{
			Exception stdin_exc = null;
			var info = new ProcessStartInfo (path, args);
			info.UseShellExecute = false;
			info.RedirectStandardInput = false;
			info.RedirectStandardOutput = true;
			info.RedirectStandardError = true;
			info.WorkingDirectory = currentDirectory;
			System.Threading.ManualResetEvent stdout_completed = new System.Threading.ManualResetEvent (false);
			System.Threading.ManualResetEvent stderr_completed = new System.Threading.ManualResetEvent (false);

			if (output == null)
				output = new StringBuilder ();

			if (env != null)
			{
				if (env.Length % 2 != 0)
					throw new Exception ("You passed an environment key without a value");

				for (int i = 0; i < env.Length; i += 2)
					info.EnvironmentVariables[env[i]] = env[i + 1];
			}

			if (Verbose)
				Console.WriteLine ("{0} {1}", path, args);

			using (var p = Process.Start (info))
			{

				p.OutputDataReceived += (s, e) => {
					if (e.Data != null)
					{
						lock (output)
							output.AppendLine (e.Data);
					}
					else
					{
						stdout_completed.Set ();
					}
				};

				p.ErrorDataReceived += (s, e) => {
					if (e.Data != null)
					{
						lock (output)
							output.AppendLine (e.Data);
					}
					else
					{
						stderr_completed.Set ();
					}
				};

				p.BeginOutputReadLine ();
				p.BeginErrorReadLine ();

				p.WaitForExit ();

				stderr_completed.WaitOne (TimeSpan.FromSeconds (1));
				stdout_completed.WaitOne (TimeSpan.FromSeconds (1));

				if (p.ExitCode != 0)
				{
					// note: this repeat the failing command line. However we can't avoid this since we're often
					// running commands in parallel (so the last one printed might not be the one failing)
					if (!suppressPrintOnErrors)
						Console.Error.WriteLine ("Process exited with code {0}, command:\n{1} {2}{3}", p.ExitCode, path, args, output.Length > 0 ? "\n" + output.ToString () : string.Empty);
					return p.ExitCode;
				}
				else if (Verbose && output.Length > 0 && !suppressPrintOnErrors)
				{
					Console.WriteLine (output.ToString ());
				}

				if (stdin_exc != null)
					throw stdin_exc;
			}

			return 0;
		}
	}
}
