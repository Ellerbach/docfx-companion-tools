// Licensed to DocFX Companion Tools and contributors under one or more agreements.
// DocFX Companion Tools and contributors licenses this file to you under the MIT license.

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
        public string DocFolder { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether verbose information is shown in the output.
        /// </summary>
        public bool Verbose { get; set; }

        /// <summary>
        /// Gets or sets the translator Azure Cognitive Services key.
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// Gets or sets the translator Azure Cognitive Services location.
        /// </summary>
        public string Location { get; set; }

        /// <summary>
        /// Gets or sets the source language.
        /// </summary>
        public string SourceLanguage { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to only check files are missing.
        /// </summary>
        public bool CheckOnly { get; set; }
    }
}
