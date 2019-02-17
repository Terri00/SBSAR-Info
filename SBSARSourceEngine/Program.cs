using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SBSARInfo;
using SBSArchive;

namespace SBSARSourceEngine
{
    /// <summary>
    /// Automatically convert .sbsar changes into vmt files.
    /// </summary>
    class Program
    {
        //static string target_location = @"D:\SteamLibrary\steamapps\common\Counter-Strike Global Offensive\csgo\materials\";
        static string target_location = @"C:\Program Files (x86)\Steam\steamapps\common\Counter-Strike Global Offensive\csgo\materials";

        static void Main(string[] args)
        {
            if (args.Count() > 0) target_location = args[0]; // Use path from args if availible.

            // Create a new FileSystemWatcher and set its properties.
            using (FileSystemWatcher watcher = new FileSystemWatcher())
            {
                watcher.Path = target_location;

                // Watch for changes in LastAccess and LastWrite times, and
                // the renaming of files or directories.
                watcher.NotifyFilter = NotifyFilters.LastAccess
                                     | NotifyFilters.LastWrite
                                     | NotifyFilters.FileName
                                     | NotifyFilters.DirectoryName;

                // Only watch text files.
                watcher.Filter = "*.sbsar";

                // Add event handlers.
                watcher.Changed += OnCreate;
                watcher.Created += OnCreate;
                watcher.Deleted += OnChanged;
                watcher.Renamed += OnRenamed;

                watcher.IncludeSubdirectories = true;

                // Begin watching.
                watcher.EnableRaisingEvents = true;

                // Wait for the user to quit the program.
                Console.WriteLine("Press 'q' to stop tracking.");
                while (Console.Read() != 'q') ;
            }
        }

        static List<string> queue = new List<string>();

        private static void OnCreate(object source, FileSystemEventArgs e){

            //Your File
            var f = new FileInfo(e.FullPath);

            while (IsFileLocked(f)) { }

            f = new FileInfo(e.FullPath);

            string id = e.FullPath + f.LastWriteTime.Ticks;
            if (queue.Contains(id)) return;
            queue.Add(id);

            SBSAR file = SBSAR.fromSBSARPackage(e.FullPath);

            string target_directory = Path.GetDirectoryName(e.FullPath);
            string archive_name = Path.GetFileNameWithoutExtension(e.FullPath);

            string relativeDirectory = Path.GetDirectoryName(e.FullPath.Substring(target_location.Length)); //Get relative directory

            //Render maps out
            foreach(Graph g in file.graphs)
            {
                string material_name = String.Format("{0}_{1}.vmt", archive_name, g.label);

                File.WriteAllText(target_directory + "/" + material_name, 
"LightmappedGeneric\n"+
"{\n"+
$"	$basetexture \"{relativeDirectory+"\\"+archive_name+"_"+g.label+"_basecolor"}\"\n"+
$"	$normal \"{relativeDirectory + "\\" + archive_name + "_" + g.label + "_normal"}\"\n" +
"}");

                Console.WriteLine("Rendering graph {0}", g.label);
                //g.renderThisToDirectory(target_directory, 11, 11);
                
                foreach(Output o in g.outputs)
                {
                    if (o.identifier == "basecolor") { o.renderThisToDirectory(target_directory); VTFCmd.CallConvert(new VTFOptions
                    { filepath = target_directory + "/" + archive_name + "_" + g.label + "_basecolor.tga", format=VTF_FORMAT.DXT1 }); }

                    if (o.identifier == "normal") { o.renderThisToDirectory(target_directory); VTFCmd.CallConvert(new VTFOptions
                    { filepath = target_directory + "/" + archive_name + "_" + g.label + "_normal.tga", format = VTF_FORMAT.DXT1 }); };
                }
            }
        }

        static bool IsFileLocked(FileInfo file)
        {
            FileStream stream = null;

            try
            {
                stream = file.Open(FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            }
            catch (IOException)
            {
                return true;
            }
            finally
            {
                if (stream != null)
                    stream.Close();
            }

            //file is not locked
            return false;
        }

        // Define the event handlers.
        private static void OnChanged(object source, FileSystemEventArgs e) =>
            // Specify what is done when a file is changed, created, or deleted.
            Console.WriteLine($"File: {e.FullPath} {e.ChangeType}");

        private static void OnRenamed(object source, RenamedEventArgs e) =>
            // Specify what is done when a file is renamed.
            Console.WriteLine($"File: {e.OldFullPath} renamed to {e.FullPath}");
    }
}
