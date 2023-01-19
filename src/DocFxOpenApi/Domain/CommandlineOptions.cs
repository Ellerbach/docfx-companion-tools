// Licensed to DocFX Companion Tools and contributors under one or more agreements.
// DocFX Companion Tools and contributors licenses this file to you under the MIT license.

namespace DocFxOpenApi.Domain
{
    using CommandLine;

    /// <summary>
    /// Class for command line options.
    /// </summary>
    public class CommandlineOptions
    {
        /// <summary>
        /// Gets or sets the folder with specifications.
        /// </summary>
        [Option('s', "specfolder", Required = true, HelpText = "Folder containing the OpenAPI specification.")]
        public string? SpecFolder { get; set; }

        /// <summary>
        /// Gets or sets the output folder.
        /// </summary>
        [Option('o', "outputfolder", Required = false, HelpText = "Folder to write the resulting specifications in.")]
        public string? OutputFolder { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether verbose information is shown in the output.
        /// </summary>
        [Option('v', "verbose", Required = false, HelpText = "Show verbose messages.")]
        public bool Verbose { get; set; }
    }
}
