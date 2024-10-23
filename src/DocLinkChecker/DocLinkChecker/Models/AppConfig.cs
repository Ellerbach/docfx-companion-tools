using System.Text.Json.Serialization;

namespace DocLinkChecker.Models
{
    /// <summary>
    /// Model for application configuration.
    /// </summary>
    public class AppConfig
    {
        /// <summary>
        /// Gets or sets the configuration file that was read.
        /// NOTE: not serialized in settings.
        /// </summary>
        [JsonIgnore]
        public string ConfigFilePath { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets a value indicating whether we have verbose output.
        /// NOTE: not serialized in settings, just a flag.
        /// </summary>
        [JsonIgnore]
        public bool Verbose { get; set; }

        /// <summary>
        /// Gets or sets the documentation files to scan.
        /// </summary>
        public FileMappingItem DocumentationFiles { get; set; } = new() { Files = { "**/*.md" } };

        /// <summary>
        /// Gets or sets the resource folder names (like .attachments, images or such).
        /// Default is '.attachments' for backward compatability.
        /// </summary>
        public List<string> ResourceFolderNames { get; set; } = new() { ".attachments" };

        /// <summary>
        /// Gets or sets the DocLinkChecher settings.
        /// </summary>
        public DocLinkCheckerSettings DocLinkChecker { get; set; } = new();

        /// <summary>
        /// Return all app settings as a string.
        /// </summary>
        /// <returns>String with settings.</returns>
        public override string ToString()
        {
            string result = string.Empty;
            if (!string.IsNullOrEmpty(ConfigFilePath))
            {
                result += $"Config file: {ConfigFilePath}\n";
            }

            result += $"Documents folder: {DocumentationFiles.SourceFolder}\n";
            if (DocumentationFiles.Files.Any())
            {
                result += $"   - inlude: {string.Join(',', DocumentationFiles.Files)}\n";
            }

            if (DocumentationFiles.Exclude.Any())
            {
                result += $"   - exclude: {string.Join(',', DocumentationFiles.Exclude)}\n";
            }

            result += $"Valid resource folder names: {string.Join(",", ResourceFolderNames)}\n";
            result += DocLinkChecker.ToString();
            return result;
        }
    }
}
