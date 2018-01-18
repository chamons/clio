using System.Collections.Generic;

namespace clio.Model
{
	/// <summary>
	/// Minimum required functionality to be able to parse a commit for bug information
	/// </summary>
	public interface ICommitParser
	{
		/// <summary>
		/// Parses a single commit and returns an enunerable of parsed commits that reference
		/// potential bugs fixed with that commit. Can return more than one if the commit
		/// references several issues.
		/// </summary>
		IEnumerable<ParsedCommit> ParseSingle (CommitInfo commit);
	}
}
