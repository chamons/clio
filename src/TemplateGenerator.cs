﻿using System;
using System.IO;
using System.Reflection;
using System.Text;
using clio.Model;

namespace clio
{
	public static class TemplateGenerator
	{
		public static void GenerateReleaseNotes (BugCollection bugCollection, SearchOptions options)
		{
			string template = GetTemplateText (options.Template.ValueOr (""));

			template = template.Replace ("{START_RANGE}", options.Starting.ValueOr (""));
			template = template.Replace ("{END_RANGE}", options.Ending.ValueOr (""));
			template = template.Replace ("{INCLUDE_STARTING}", options.IncludeStarting ? "True" : "False");

			template = template.Replace ("{BUG_LIST}", GetBugReportText (bugCollection, options));

			File.WriteAllText (options.OutputPath, template);
		}

		public static bool ValidateTemplateName (string name)
		{
			return GetTemplateStream (name) != null;
		}

		static string GetTemplateText (string name)
		{
			using (Stream stream = GetTemplateStream (name))
			{
				if (stream == null)
				{
					Console.Error.WriteLine ($"Unable to find template {name} embedded as a resource.");
					Environment.Exit (-1);
				}

				using (StreamReader reader = new StreamReader (stream))
				{
					return reader.ReadToEnd ();
				}
			}
		}

		static string GetBugReportText (BugCollection bugCollection, SearchOptions options)
		{
			StringBuilder builder = new StringBuilder ();

			foreach (var bug in bugCollection.ConfirmedBugs)
				builder.AppendLine (FormatBug (bug, options));
			builder.AppendLine ();

			if (bugCollection.UncertainBugs.Count > 0)
			{
				builder.AppendLine ("XXX - Potential bugs (for manual review)");

				foreach (var bug in bugCollection.UncertainBugs)
					builder.AppendLine (FormatUncertainBug (bug, options));
			}

			return builder.ToString ();
		}

		static string FormatBug (BugEntry bug, SearchOptions options)
		{
			switch (options.Template.ValueOr (""))
			{
				case "Android":
					return $"* [{bug.ID}](https://bugzilla.xamarin.com/show_bug.cgi?id={bug.ID}):\n\t{bug.Title}" + (String.IsNullOrEmpty (bug.SecondaryTitle) ? "" : $" / {bug.SecondaryTitle}");
				case "Mac":
				case "iOS":
				default:
					return $"* [{bug.ID}](https://bugzilla.xamarin.com/show_bug.cgi?id={bug.ID}) -  {bug.Title}" + (String.IsNullOrEmpty (bug.SecondaryTitle) ? "" : $" / {bug.SecondaryTitle}");
			}
		}

		static string FormatUncertainBug (BugEntry bug, SearchOptions options)
		{
			switch (options.Template.ValueOr (""))
			{
				case "Android":
					return $"* [{bug.ID}](https://bugzilla.xamarin.com/show_bug.cgi?id={bug.ID}):\n\t{bug.SecondaryTitle}";
				case "Mac":
				case "iOS":
				default:
					return $"* [{bug.ID}](https://bugzilla.xamarin.com/show_bug.cgi?id={bug.ID}) -  {bug.SecondaryTitle}";
			}
		}

		static Stream GetTemplateStream (string name)
		{
			var assembly = Assembly.GetExecutingAssembly ();
			var resourceName = $"clio.Templates.{name}.md";
			return assembly.GetManifestResourceStream (resourceName);
		}
	}
}
