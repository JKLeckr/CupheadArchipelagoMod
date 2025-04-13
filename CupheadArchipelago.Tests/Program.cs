using System;
using System.Collections.Generic;
using NUnitLite;

namespace CupheadArchipelago.Tests {
    public class TestMain {
        public static int Main(string[] args) {
            List<string> largs = new List<string>(args) {
                "--noresult",
            };
            Console.WriteLine($"Running NUnitLite with args [{ArgsToString(largs)}]");
            Console.WriteLine();
            return new AutoRun().Execute(largs.ToArray());
        }

        private static string ArgsToString(IEnumerable<string> args) {
            string res = "";
            bool sep = false;
            foreach (string arg in args) {
                if (sep) res += " ";
                res += $"\"{arg}\"";
                sep = true;
            }
            return res;
        }
    }
}
