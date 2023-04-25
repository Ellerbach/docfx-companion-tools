// Licensed to DocFX Companion Tools and contributors under one or more agreements.
// DocFX Companion Tools and contributors licenses this file to you under the MIT license.

namespace DocLinkChecker.Models
{
    using CommandLine;

    /// <summary>
    /// Class for command line options when no config file is available.
    /// This options are the ones of the "old" implementation. This is kept
    /// to be backwards compatible. In this version we have required options.
    /// </summary>
    public class CommandLineOptions
    {
        /// <summary>
        /// Gets or sets the folder with documents.
        /// </summary>
        [Option('d', "docfolder", Required = true, HelpText = "Folder containing the documents.")]
        public string DocFolder { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the configuration file.
        /// </summary>
        [Option('f', "config", Required = false, HelpText = "Configuration file to load for settings.")]
        public string ConfigFilePath { get; set; } = string.Empty;

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

        /// <summary>
        /// Gets or sets a value indicating whether to validate external links.
        /// </summary>
        [Option('x', "external", Required = false, HelpText = "Validate links to external sources.")]
        public bool ValidateExternalLinks { get; set; }
    }
}
