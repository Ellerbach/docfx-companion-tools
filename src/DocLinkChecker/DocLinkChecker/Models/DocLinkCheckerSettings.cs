﻿using DocLinkChecker.Enums;

namespace DocLinkChecker.Models
{
    /// <summary>
    /// Model class for doc link checker settings.
    /// </summary>
    public class DocLinkCheckerSettings
    {
        /// <summary>
        /// Gets or sets the relative link strategy to validate.
        /// Default is <see cref="RelativeLinkType.All"/> for backwards compatibility.
        /// </summary>
        public RelativeLinkType RelativeLinkStrategy { get; set; } = RelativeLinkType.All;

        /// <summary>
        /// Gets or sets a value indicating whether resources actually used.
        /// </summary>
        public bool CheckForOrphanedResources { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether orphaned resource files should be deleted.
        /// </summary>
        public bool CleanupOrphanedResources { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether pipe tables are validated for proper formatting.
        /// </summary>
        public bool ValidatePipeTableFormatting { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to validate external links.
        /// Default is false for backward compatability.
        /// </summary>
        public bool ValidateExternalLinks { get; set; }

        /// <summary>
        /// Gets or sets the concurrency level, which is the number of concurrent workers.
        /// </summary>
        public int ConcurrencyLevel { get; set; } = 5;

        /*/// <summary>
        /// Gets or sets the maximum number of HTTP redirects that will be handled
        /// to prevent getting stuck in redirection loops, but still handle stacked redirects.
        /// </summary>
        public int MaxHttpRedirects { get; set; } = 20; */

        /// <summary>
        /// Gets or sets the number of milliseconds that will trigger a warning of links taking a long time.
        /// </summary>
        public int ExternalLinkDurationWarning { get; set; } = 3000;

        /// <summary>
        /// Gets or sets the list of URL's to whitelist. Items have to start with http or https.
        /// But just adding the domain name is enough to whitelist all URLs from that domain.
        /// </summary>
        public List<string> WhitelistUrls { get; set; } = new List<string>() { "http://localhost" };

        /// <summary>
        /// Return all app settings as a string.
        /// </summary>
        /// <returns>String with settings.</returns>
        public override string ToString()
        {
            string result = $"Relative link strategy: {RelativeLinkStrategy}\n";
            result += $"Check for orphaned resources: {CheckForOrphanedResources}\n";
            result += $"Cleanup orphaned resources: {CleanupOrphanedResources}\n";
            result += $"Validate pipe table formatting: {ValidatePipeTableFormatting}\n";
            result += $"Validate external links: {ValidateExternalLinks}\n";
            result += $"Concurrency level: {ConcurrencyLevel}\n";
            ////result += $"Max HTTP redirects: {MaxHttpRedirects}\n";
            result += $"Whitelist URLs: {string.Join(",", WhitelistUrls)}\n";
            return result;
        }
    }
}
