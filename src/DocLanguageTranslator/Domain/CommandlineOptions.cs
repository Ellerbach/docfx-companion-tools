// Licensed to DocFX Companion Tools and contributors under one or more agreements.
// DocFX Companion Tools and contributors licenses this file to you under the MIT license.
using CommandLine;

namespace DocFXLanguageGenerator.Domain
{
    /// <summary>
    /// Class for command line options.
    /// </summary>
    public class CommandlineOptions
    {
        /// <summary>
        /// Gets or sets the folder with documents.
        /// </summary>
        [Option('d', "docfolder", Required = true, HelpText = "Folder containing the documents.")]
        public string DocFolder { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether verbose information is shown in the output.
        /// </summary>
        [Option('v', "verbose", Required = false, HelpText = "Show verbose messages.")]
        public bool Verbose { get; set; }

        /// <summary>
        /// Gets or sets the translator Azure Cognitive Services key.
        /// </summary>
        [Option('k', "key", Required = false, HelpText = "The translator Azure Cognitive Services key.")]
        public string Key { get; set; }

        /// <summary>
        /// Gets or sets the translator Azure Cognitive Services key.
        /// </summary>
        [Option('l', "location", Required = false, HelpText = "The translator Azure Cognitive Services location, default is westeurope.")]
        public string Location { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to only check files are missing.
        /// </summary>
        [Option('c', "check", Required = false, HelpText = "The translator Azure Cognitive Services key.")]
        public bool CheckOnly { get; set; }
    }
}
