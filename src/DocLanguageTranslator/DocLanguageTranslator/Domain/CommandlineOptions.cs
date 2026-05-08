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

        /// <summary>
        /// Gets or sets the source file path for line range translation.
        /// </summary>
        public string SourceFile { get; set; }

        /// <summary>
        /// Gets or sets the line range to translate (e.g., "1-10" or "5-20").
        /// </summary>
        public string LineRange { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether translated lines should be inserted
        /// at the specified position instead of replacing existing lines in the target file.
        /// </summary>
        public bool InsertLines { get; set; }

        /// <summary>
        /// Gets or sets the target language codes to translate to.
        /// When specified, only these languages are used as translation targets.
        /// When null or empty, languages are auto-discovered from folder names.
        /// </summary>
        public string[] TargetLanguages { get; set; }
    }
}
