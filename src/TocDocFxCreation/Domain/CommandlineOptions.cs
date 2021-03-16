// Licensed to DocFX Companion Tools and contributors under one or more agreements.
// DocFX Companion Tools and contributors licenses this file to you under the MIT license.

namespace TocDocFxCreation.Domain
{
    using CommandLine;

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
        /// Gets or sets the folder with documents.
        /// </summary>
        [Option('o', "outputfolder", Required = false, HelpText = "Folder to write the resulting toc.yml in.")]
        public string OutputFolder { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether verbose information is shown in the output.
        /// </summary>
        [Option('v', "verbose", Required = false, HelpText = "Show verbose messages.")]
        public bool Verbose { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the .order files are used.
        /// </summary>
        [Option('s', "sequence", Required = false, HelpText = "Use the .order files for TOC sequence. Format are raws of: filename-without-extension")]
        public bool UseOrder { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the .order files are used.
        /// </summary>
        [Option('r', "override", Required = false, HelpText = "Use the .override files for TOC file name override. Format are raws of: filename-without-extension;Title you want")]
        public bool UseOverride { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether an index is automatically added.
        /// </summary>
        [Option('i', "index", Required = false, HelpText = "Auto-generate a file index in each folder.")]
        public bool AutoIndex { get; set; }
    }
}
