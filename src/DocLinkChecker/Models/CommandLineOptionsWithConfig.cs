// Licensed to DocFX Companion Tools and contributors under one or more agreements.
// DocFX Companion Tools and contributors licenses this file to you under the MIT license.

namespace DocLinkChecker.Models
{
    using CommandLine;

    /// <summary>
    /// Class for command line options. These options are used when we have a config
    /// file available (the new way). In this case we want to check options to
    /// enable overwrite. But none of the options are required.
    /// </summary>
    public class CommandLineOptionsWithConfig
    {
        /// <summary>
        /// Gets or sets the folder with documents.
        /// </summary>
        [Option('d', "docfolder", Required = false, HelpText = "Folder containing the documents.")]
        public string DocFolder { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether verbose information is shown in the output.
        /// </summary>
        [Option('v', "verbose", Required = false, HelpText = "Show verbose messages.")]
        public bool Verbose { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether we need to check .attachments folder for unreferenced files.
        /// </summary>
        [Option('a', "attachments", Required = false, HelpText = "Check unreferenced files in .attachments.")]
        public bool Attachments { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to cleanup unreferenced files in .attachments.
        /// </summary>
        [Option('c', "cleanup", Required = false, HelpText = "Cleanup unreferenced files in .attachments.")]
        public bool Cleanup { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to check that tables are well formed.
        /// </summary>
        [Option('t', "table", Required = false, HelpText = "Check that tables are well formed.")]
        public bool Table { get; set; }
    }
}
