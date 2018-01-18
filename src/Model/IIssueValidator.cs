using System.Collections.Generic;
using System.Threading.Tasks;

namespace clio.Model
{
	/// <summary>
	/// Represents a class that can validate a sequence of ParsedCommit instances
	/// </summary>
	public interface IIssueValidator
	{
		/// <summary>
		/// Takes the given enumerable of commits and validates the issues found for each. Returns
		/// a new enumerable where the confidence of the parsed commit is adjusted up or down.
		/// </summary>
		Task<IEnumerable<ParsedCommit>> ValidateIssuesAsync (IEnumerable<ParsedCommit> commits);
	}
}
