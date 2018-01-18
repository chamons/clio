namespace clio.Model
{
	public enum ParsingConfidence
	{
		/// <summary>
		/// This is clearly a bug for the given issue provider, most likely because it
		/// is referenced via the full url or the prefix for the bug clearly denotes it
		/// as such or the bug was verified to exist for the issue provider.
		/// </summary>
		High,

		/// <summary>
		/// This is probably a bug for the given issue provider but we have not yet verified it
		/// </summary>
		Likely,

		/// <summary>
		/// This looks like a bug but verification failed to identify a match
		/// </summary>
		Low,

		/// <summary>
		/// No bug identified
		/// </summary>
		Invalid,
	}
}
