namespace DocLinkChecker.Models
{
    using System.Collections.Generic;
    using System.Text.Json.Serialization;

    /// <summary>
    /// Model for application configuration.
    /// </summary>
    public class AppConfig
    {
        /// <summary>
        /// Gets or sets the documents folder to scan.
        /// NOTE: not serialized in settings.
        /// </summary>
        [JsonIgnore]
        public string DocumentsFolder { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets a value indicating whether we have verbose output.
        /// NOTE: not serialized in settings, just a flag.
        /// </summary>
        [JsonIgnore]
        public bool Verbose { get; set; }

        /// <summary>
        /// Gets or sets the configuration file that was read.
        /// NOTE: not serialized in settings.
        /// </summary>
        [JsonIgnore]
        public string ConfigFilePath { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the resource folder names (like .attachments, images or such).
        /// </summary>
        public List<string> ResourceFolderNames { get; set; } = new () { ".attachments", "images" };

        /// <summary>
        /// Gets or sets the DocLinkChecher settings.
        /// </summary>
        public DocLinkCheckerSettings DocLinkChecker { get; set; } = new ();

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

            result += $"Documents folder: {DocumentsFolder}\n";
            result += $"Valid resource folder names: {string.Join(",", ResourceFolderNames)}\n";
            result += DocLinkChecker.ToString();
            return result;
        }
    }
}
