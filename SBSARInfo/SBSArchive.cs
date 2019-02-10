﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;
using System.Xml;
using System.ComponentModel;

public static class ext{
    public static T GetValue<T>(this XmlAttributeCollection dict, string key, T defaultValue = default(T))
    {
        if (dict[key] != null){
            TypeConverter typeConverter = TypeDescriptor.GetConverter(typeof(T));
            return (T)(typeConverter.ConvertFromString(dict[key].Value));
        }
        else
            return defaultValue;
    }
}

namespace SBSArchive
{
    public class Output
    {
        public long uid;
        public string identifier;
        public int format;
        public int width;
        public int height;
        public int mipmaps;
        public bool dynamicsize;

        public Output(XmlNode n)
        {
            // Load output information
            this.uid = n.Attributes.GetValue<long>("uid");
            this.identifier = n.Attributes.GetValue<string>("identifier");
            this.format = n.Attributes.GetValue<int>("format");
            this.width = n.Attributes.GetValue<int>("width");
            this.height = n.Attributes.GetValue<int>("height");
            this.mipmaps = n.Attributes.GetValue<int>("mipmaps");
            this.dynamicsize = n.Attributes.GetValue<string>("dynamicsize", "") == "yes";
        }

        public void printInfo(string indent = "      ")
        {
            Console.WriteLine(
                "{0}Output info [{1}]\n" +
                "{0}  Format:       {2}\n" +
                "{0}  Width:        {3}\n" +
                "{0}  Height:       {4}\n" +
                "{0}  Mipmaps:      {5}\n" +
                "{0}  Dynamic Size: {6}",

                indent,
                this.identifier,
                this.format, this.width, this.height, this.mipmaps, this.dynamicsize ? "yes" : "no"
                );
        }
    }

    public class Input
    {
        public long uid;
        public string identifier;
        public int type;
        public string _default;
        public long[] alteroutputs;
        public int alternodes;

        public Input(XmlNode n)
        {
            // Load input information
            this.uid = n.Attributes.GetValue<long>("uid");
            this.identifier = n.Attributes.GetValue<string>("identifier");
            this.type = n.Attributes.GetValue<int>("type");
            this._default = n.Attributes.GetValue<string>("default");
            this.alteroutputs = (n.Attributes.GetValue<string>("alteroutputs", "").Length > 0) ? 
                n.Attributes.GetValue<string>("alteroutputs", "").Split(',').ToList().Select(s => long.Parse(s)).ToArray() : new long[0]; // ew

            this.alternodes = n.Attributes.GetValue<int>("alternodes");
        }

        public void printInfo(string indent = "      ")
        {
            Console.WriteLine(
                "{0}Input info [{1}]\n" +
                                        "{0}  UID:              {2}\n" +
                                        "{0}  type:             {3}\n" +
                (this._default!=null?   "{0}  default val:      {4}\n":"") +
                                        "{0}  Affects outputs:  {5}\n" +
                                        "{0}  Affects nodes:    {6}",

                indent,
                this.identifier,
                this.uid, this.type, this._default, String.Join(", ", this.alteroutputs), this.alternodes
                );
        }
    }

    public class Graph
    {
        public string pkgurl;
        public string label;
        public string[] keywords;
        public string description;
        public string category;
        public string author;
        public string[] usertags;

        public Output[] outputs;
        public Input[] inputs;

        public Graph(XmlNode n)
        {
            // Load graph information
            this.pkgurl = n.Attributes.GetValue<string>("pkgurl");
            this.label = n.Attributes.GetValue<string>("label");
            this.keywords = n.Attributes.GetValue<string>("keywords", "").Split(';');
            this.description = n.Attributes.GetValue<string>("description");
            this.category = n.Attributes.GetValue<string>("category");
            this.author = n.Attributes.GetValue<string>("author");
            this.usertags = n.Attributes.GetValue<string>("usertag", "").Split(';');

            // Allocate output array
            this.outputs = new Output[int.Parse(n.SelectSingleNode("outputs").Attributes["count"].Value)];

            // Load all output informations
            int i = 0;
            foreach (XmlNode output in n.SelectSingleNode("outputs").SelectNodes("output"))
            {
                this.outputs[i++] = new Output(output);
            }

            // Allocate input array
            this.inputs = new Input[int.Parse(n.SelectSingleNode("inputs").Attributes["count"].Value)];

            // load all input informations
            i = 0;
            foreach (XmlNode input in n.SelectSingleNode("inputs").SelectNodes("input"))
            {
                this.inputs[i++] = new Input(input);
            }

        }

        // Get output by its UID
        public Output getOutputByUid(long uid)
        {
            foreach (Output o in this.outputs) if (o.uid == uid) return o;
            return null;
        }

        public void printInfo(string indent = "    ")
        {
            Console.WriteLine(
                "{0}Graph info [{1}]\n" +
                (this.pkgurl!=null?         "{0}  Package URL:  {2}\n": "") +
                                            "{0}  Keywords:     {3}\n" +
                (this.description!=null?    "{0}  Description:  {4}\n": "") +
                (this.category!=null?       "{0}  Category:     {5}\n": "") +
                (this.author!=null?         "{0}  Author:       {6}\n": "") +
                                            "{0}  Usertags:     {7}",

                indent,
                this.label,
                this.pkgurl, String.Join(", ", this.keywords), this.description, this.category, this.author, String.Join(", ", this.usertags)
                );
        }
    }

    public class SBSAR
    {
        public string filename; // Not part of actual sbsar spec

        public double formatversion;
        public long asmuid;
        public long cookerbuild;
        public string content;

        Graph[] graphs;

        /// <summary> Load sbsar from XML document </summary>
        /// <param name="filename">XML File</param>
        private SBSAR(string filename)
        {
            this.filename = filename;

            XmlDocument doc = new XmlDocument();
            doc.LoadXml(File.ReadAllText(filename));

            // Read SBS description
            XmlNode desc = doc.GetElementsByTagName("sbsdescription")[0];
            this.formatversion = desc.Attributes.GetValue<double>("formatversion");
            this.asmuid = desc.Attributes.GetValue<long>("asmuid");
            this.cookerbuild = desc.Attributes.GetValue<long>("cookerbuild");
            this.content = desc.Attributes.GetValue<string>("content");

            // Allocate graph array
            this.graphs = new Graph[int.Parse(desc.SelectSingleNode("graphs").Attributes["count"].Value)];

            // load all graphs in file
            int i = 0;
            foreach (XmlNode graph in desc.SelectSingleNode("graphs").SelectNodes("graph"))
            {
                this.graphs[i++] = new Graph(graph);
            }
        }

        /// <summary>Loads archive information from .sbsar file</summary>
        /// <param name="filename">.sbsar file</param>
        /// <returns>Parsed archive info</returns>
        public static SBSAR fromSBSARPackage(string filename, bool muteLZMA = true)
        {
            string temp_guid = Guid.NewGuid().ToString(); // Create an id for temporary files
            string temp_folder = String.Format(@"\temp\{0}\", temp_guid); // Create temporary folder
            Directory.CreateDirectory(temp_folder);

            // Substance archives are compressed with LZMA, use 7zip to extract
            var proc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "7z.exe",
                    Arguments = String.Format("x {0} -y -o{1}", filename, temp_folder),
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    RedirectStandardInput = true
                }
            };

            // Send all the debug to console (if we want it)
            if (!muteLZMA) proc.OutputDataReceived += (sender, rec) => Console.WriteLine("{0}", rec.Data);
            proc.Start();
            proc.BeginOutputReadLine();
            proc.WaitForExit();

            // Load SBSAR file from found XML document
            string[] xmlDocuments = Directory.GetFiles(temp_folder, "*.xml", SearchOption.AllDirectories);

            if (xmlDocuments.Count() == 0) throw new Exception("No .xml files found within archive");
            if (xmlDocuments.Count() > 1) Console.WriteLine("Warning: More than one .xml files found, using first as archive");

            SBSAR file = SBSAR.fromXMLDocument(xmlDocuments[0]);
            Directory.Delete(temp_folder, true);
            return file;
        }

        /// <summary>Loads archive information from .xml file</summary>
        /// <param name="filename">.xml file</param>
        /// <returns>Parsed archive info</returns>
        public static SBSAR fromXMLDocument(string document)
        {
            return new SBSAR(document);
        }

        public void printInfo(string indent = "")
        {
            Console.WriteLine(
                "{0}SBSAR file info [{5}]\n" +
                "{0}  Format Version:   {1}\n" +
                "{0}  ASM UID:          {2}\n" +
                "{0}  Cooker Build:     {3}\n" +
                "{0}  Content:          {4}",
                indent,
                this.formatversion, this.asmuid, this.cookerbuild, this.content,
                Path.GetFileNameWithoutExtension(this.filename));
        }

        public void printFullInfo(bool onlyGraphs = true)
        {
            this.printInfo();

            Console.WriteLine("\n  GRAPHS ::");

            foreach (Graph g in this.graphs)
            {
                g.printInfo();

                if (!onlyGraphs)
                {
                    Console.WriteLine("\n    OUTPUTS ::");

                    foreach (Output o in g.outputs)
                    {
                        o.printInfo(); Console.WriteLine();
                    }

                    Console.WriteLine("    INPUTS ::");

                    foreach (Input i in g.inputs)
                    {
                        i.printInfo(); Console.WriteLine();
                    }
                }
            }
        }
    }
}