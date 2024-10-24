using System.Text.Json.Serialization;

namespace DocLinkChecker.Models
{
    /// <summary>
    /// Model class for file mapping.
    /// </summary>
    public class FileMappingItem
    {
        /// <summary>
        /// Gets or sets the source folder.
        /// </summary>
        [JsonPropertyName("src")]
        public string SourceFolder { get; set; }

        /// <summary>
        /// Gets or sets the folders and files to include.
        /// This list supports the file glob pattern.
        /// </summary>
        public List<string> Files { get; set; } = new();

        /// <summary>
        /// Gets or sets the folders and files to exclude.
        /// This list supports the file glob pattern.
        /// </summary>
        public List<string> Exclude { get; set; } = new();
    }
}
