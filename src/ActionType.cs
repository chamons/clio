namespace clio
{
	public enum ActionType
	{
		/// <summary>
		/// Shows the command line help
		/// </summary>
		Help,

		/// <summary>
		/// Lists the commits that are for consideration
		/// </summary>
		ListConsideredCommits,

		/// <summary>
		/// Lists the bugs found from the considered commits
		/// </summary>
		ListBugs,

		/// <summary>
		/// Exports the bugs found from the considered commits
		/// </summary>
		ExportBugs
	}
}
