using System;
using System.IO;
using System.Reflection;

namespace clio.Tests
{
	public static class TestDataLocator
	{
		public static string GetPath ()
		{
			return Path.Combine (AssemblyDirectory, "../../../clio-test-data");
		}

		public static string AssemblyDirectory
		{
			get 
			{
				string codeBase = Assembly.GetExecutingAssembly ().CodeBase;
				UriBuilder uri = new UriBuilder (codeBase);
				string path = Uri.UnescapeDataString (uri.Path);
				return Path.GetDirectoryName (path);
			}
		}
	}
}
