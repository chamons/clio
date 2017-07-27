﻿using System;
using System.IO;
using Optional;

namespace clio
{
	public class SearchOptions
	{
		public string OutputPath { get; set; } = Path.Combine (System.Environment.CurrentDirectory, "ReleaseNotes.md");
		public Option<string> Template { get; set; } = Option.None<string> ();

		public bool DisableBugzillaValidation { get; set; } = false;

		public bool IgnoreLowBugs { get; set; } = true;
		public bool Explain { get; set; } = false;
		public Option<string> SingleCommit { get; set; } = Option.None<string> ();

		public Option<string> Oldest { get; set; } = Option.None<string> ();
		public bool IncludeOldest { get; set; } = true;
		public Option<string> Newest { get; set; } = Option.None<string> ();
	}
}
