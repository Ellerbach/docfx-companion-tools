// Licensed to DocFX Companion Tools and contributors under one or more agreements.
// DocFX Companion Tools and contributors licenses this file to you under the MIT license.
using CommandLine;

namespace DocFxOpenApi.Domain
{
    /// <summary>
    ///  Class for command line options.
    /// </summary>
    public class CommandlineOptions
    {
        /// <summary>
        /// Gets or sets the folder with specifications.
        /// </summary>
        [Option('s', "specsource", Required = true, HelpText = "Folder or File containing the OpenAPI specification.")]
        public string? SpecSource
        {
            get => SpecFolder ?? SpecFile;
            set => SetSource(value);
        }

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

        /// <summary>
        /// Gets the folder with specifications, if the source is a folder.
        /// </summary>
        public string? SpecFolder { get; private set; }

        /// <summary>
        /// Gets the file with specifications, if the source is a file.
        /// </summary>
        public string? SpecFile { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether to generate missing OperationId members.
        /// </summary>
        [Option('g', "genOpId", Required = false, HelpText = "Generate missing OperationId members.")]
        public bool GenerateOperationId { get; set; }

        private void SetSource(string? value)
        {
            if (value == null)
            {
                return;
            }

            if (Directory.Exists(value))
            {
                SpecFolder = value;
            }
            else if (File.Exists(value))
            {
                SpecFile = value;
            }
        }
    }
}
