using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SBSARSourceEngine
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
            s += "-" + arg + " \"" + value + "\" ";

            return s;
        }
    }

    public enum VTF_FORMAT
    {
        NONE = -1,
        RGBA8888 = 0,
        ABGR8888,
        RGB888,
        BGR888,
        RGB565,
        I8,
        IA88,
        P8,
        A8,
        RGB888_BLUESCREEN,
        BGR888_BLUESCREEN,
        ARGB8888,
        BGRA8888,
        DXT1,
        DXT3,
        DXT5,
        BGRX8888,
        BGR565,
        BGRX5551,
        BGRA4444,
        DXT1_ONEBITALPHA,
        BGRA5551,
        UV88,
        UVWQ8888,
        RGBA16161616F,
        RGBA16161616,
        UVLX8888
    };

    public sealed class VTFOptions
    {
        public string filepath;
        public string folder;
        public string output_directory;
        public VTF_FORMAT format = VTF_FORMAT.BGR888;
        public VTF_FORMAT alpha_format = VTF_FORMAT.BGRA8888;
        public string flags;

        public bool resize;

        public string generateCMDLArgs()
        {
            if ((this.filepath == null) && (this.folder == null)) throw new Exception("No input set");

            string v = "";

            //Flags
            v = v.extendFlag("resize", this.resize);

            //IO
            v = this.filepath != null? v.extendArg("file", this.filepath) : v.extendArg("folder", this.folder);
            v = v.extendArg("output", this.output_directory);

            //Format
            v = v.extendArg("format", this.format.ToString());
            v = v.extendArg("alphaformat", this.format.ToString());

            return v.Substring(0, v.Length - 1); //Trim last space off because neatness
        }
    }

    public class VTFCmd
    {
        public static void CallConvert(VTFOptions info)
        {
            Console.WriteLine("Calling VTFCmd.exe");

            Console.WriteLine(info.generateCMDLArgs());

            var proc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "VTFCmd.exe",
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
