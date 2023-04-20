namespace DocLinkChecker.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Text.Json.Serialization;
    using System.Threading.Tasks;
    using CommandLine;
    using CommandLine.Text;

    /// <summary>
    /// Model for application configuration.
    /// </summary>
    internal class AppConfig
    {
        /// <summary>
        /// Gets or sets the documents folder to scan.
        /// </summary>
        public string DocumentsFolder { get; set; } = ".";

        /// <summary>
        /// Gets or sets the valid resource folder names (like .attachments, images or such).
        /// </summary>
        public List<string> ValidResourceFolderNames { get; set; } = new () { ".attachments", "images" };

        /// <summary>
        /// Gets or sets a value indicating whether resources can be stored as subfolder in any
        /// location, or just in a centralized location under the <see cref="DocumentsFolder"/>.
        /// </summary>
        public bool UseOnlyCentralizedResource { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether resources are validated that they are
        /// referenced by any document in our scan. If a resource file is orphaned, the
        /// validation will result in an error when this switch is set to true.
        /// </summary>
        public bool ValidateResources { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether orphaned resource files should be deleted.
        /// </summary>
        public bool CleanupOrphanedResources { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether pipe tables are validated for proper formatting.
        /// </summary>
        public bool ValidatePipeTableFormatting { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether we have verbose output.
        /// NOTE: not serialized in settings, just a flag.
        /// </summary>
        [JsonIgnore]
        public bool Verbose { get; set; }
    }
}
