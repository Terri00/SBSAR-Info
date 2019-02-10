using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using SBSArchive;

namespace SBSARInfo
{
    class Program
    {
        //TODO: add compiling features
        static string path_automation_toolkit = @"C:\Program Files\Allegorithmic\Substance Automation Toolkit";

        static void Main(string[] args)
        {
            if (args.Length == 0) return;
            if (Path.GetExtension(args[0]) != ".sbsar") { Console.WriteLine("Not a substance archive file"); return; }

            SBSAR file = SBSAR.fromSBSARPackage(args[0]);
            file.printFullInfo(false);
        }
    }
}
