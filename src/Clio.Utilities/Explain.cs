using System;

namespace Clio.Utilities
{
	public static class Explain
	{
		static public bool Enabled;

		static int ExplainIndent = 0;
		static public void Indent () => ExplainIndent += 1;
		static public void Deindent ()
		{
			ExplainIndent -= 1;
			if (ExplainIndent < 0)
				throw new InvalidOperationException ("Can not indent negative");
		}

		static public void Print (string s)
		{
			if (Enabled)
				Console.WriteLine (new string ('\t', ExplainIndent) + s);
		}
	}
}