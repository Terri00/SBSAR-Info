using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SBSARInfo
{
    public static partial class ext
    {
        public static string extendFlag(this string s, string flag, bool value)
        {
            if (value) s += "-" + flag + " ";

            return s;
        }

        public static string extendArg(this string s, string arg, object value)
        {
            if (value == null) return s;
            s += "--" + arg + " \"" + value + "\" ";

            return s;
        }
    }

    public sealed class RenderInfo
    {
        public bool quiet = false;
        public bool verbose = true;

        public string input;
        public string input_graph;
        public string input_graph_output;

        public Dictionary<string, string> imageTweaks;
        public Dictionary<string, string> valueTweaks;

        public string output_format = "tga";
        public string output_format_compression;
        public string output_rename;
        public string output_path;

        public string bitDepth;

        public int cpu_count = 2;
        public int memory_budget = 1000;

        public string generateCMDLArgs()
        {
            if (this.input == null) throw new Exception("No input set");

            string v = "";

            //Globals
            v = v.extendFlag("q", this.quiet);
            v = v.extendFlag("v", this.verbose);

            v += "render "; //Entering rendering options

            v = v.extendArg("cpu-count", this.cpu_count);
            v = v.extendArg("memory-budget", this.memory_budget);

            //Input
            v = v.extendArg("input", this.input);
            v = v.extendArg("input-graph", this.input_graph);
            v = v.extendArg("input-graph-output", this.input_graph_output);

            //Bit depth
            v = v.extendArg("output-bit-depth", this.bitDepth);

            //format
            v = v.extendArg("output-format", this.output_format);
            v = v.extendArg("output-format-compression", this.output_format_compression);

            //renaming
            v = v.extendArg("output-name", this.output_rename);
            v = v.extendArg("output-path", this.output_path);

            //Tweaks
            if(this.imageTweaks != null) foreach (string key in this.imageTweaks.Keys) v = v.extendArg("set-entry", String.Format("{0}@{1}", key, this.imageTweaks[key]));
            if(this.valueTweaks != null) foreach (string key in this.valueTweaks.Keys) v = v.extendArg("set-value", String.Format("{0}@{1}", key, this.valueTweaks[key]));

            return v.Substring(0, v.Length -1); //Trim last space off because neatness
        }
    }

    public class AutomationTools
    {
        static string path_toolkit_root = @"C:\Program Files\Allegorithmic\Substance Automation Toolkit\";

        //Checks the automation toolkit is propperly installed
        public static bool checkLinks(){
            bool success = true;

            Console.Write("Checking automation toolkit\t");
            if (Directory.Exists(path_toolkit_root)) Console.Write("SUCCESS\n");
            else { Console.Write("FAILED\n"); success = false; }

            Console.Write("Checking sbsrender.exe\t");
            if (File.Exists(path_toolkit_root + "sbsrender.exe")) Console.Write("SUCCESS\n");
            else { Console.Write("FAILED\n"); success = false; }

            return success;
        }

        public static void CallRender(RenderInfo info)
        {
            Console.WriteLine("Calling sbsrender.exe");
            
            var proc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = path_toolkit_root + "sbsrender.exe",
                    Arguments = info.generateCMDLArgs(),
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    RedirectStandardInput = true
                }
            };

            proc.OutputDataReceived += (sender, rec) => Console.WriteLine("{0}", rec.Data);
            proc.Start();
            proc.BeginOutputReadLine();
            proc.WaitForExit();
            proc.Close();

            Console.WriteLine("Done");
        }
    }
}
