using System;

namespace Clio.Utilities
{
    public static class Errors
    {
        public static void Die (string v)
        {
            Console.Error.WriteLine (v);
            Environment.Exit (-1);
        }
    }
}