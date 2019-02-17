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
        static void Main(string[] args)
        {
            if (args.Length == 0) return;
            if (Path.GetExtension(args[0]) != ".sbsar") { Console.WriteLine("Not a substance archive file"); return; }

            SBSAR file = SBSAR.fromSBSARPackage(args[0]);
            file.printFullInfo(false);

            return;/*
            if (!AutomationTools.checkLinks()) { Console.WriteLine("Check your Automation toolkit installation"); return; } // Check substance install

            args = new string[] { "wood_cedar_white.sbsar" };

            if (args.Length == 0) return;
            if (Path.GetExtension(args[0]) != ".sbsar") { Console.WriteLine("Not a substance archive file"); return; }

            SBSAR file = SBSAR.fromSBSARPackage(args[0]);
            //file.graphs[0].outputs[0].renderThisToDirectory("testD", 11, 11);
            file.graphs[0].renderThisToDirectory("testD", 4, 4);

            Console.ReadLine();*/
        }
    }
}