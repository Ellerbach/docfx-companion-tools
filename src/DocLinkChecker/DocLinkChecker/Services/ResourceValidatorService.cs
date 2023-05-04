namespace DocLinkChecker.Services
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using DocLinkChecker.Enums;
    using DocLinkChecker.Helpers;
    using DocLinkChecker.Models;
    using Microsoft.Extensions.FileSystemGlobbing;

    /// <summary>
    /// Resource validation service.
    /// </summary>
    public class ResourceValidatorService
    {
        private readonly AppConfig _config;
        private readonly CustomConsoleLogger _console;

        /// <summary>
        /// Initializes a new instance of the <see cref="ResourceValidatorService"/> class.
        /// </summary>
        /// <param name="config">App configuration.</param>
        /// <param name="console">Console logger.</param>
        public ResourceValidatorService(
            AppConfig config,
            CustomConsoleLogger console)
        {
            _config = config;
            _console = console;
        }

        /// <summary>
        /// Check for orphaned resources.
        /// We check all folders with the name(s) specified in the configuration, including the sub folders.
        /// If files are stored there that are not referenced in the markdown files, they are considered orphaned.
        /// </summary>
        /// <param name="links">Used links.</param>
        /// <returns>List of unused resources and a list of errors.</returns>
        public (List<string> unusedResources, List<MarkdownError> errors) CheckForOrphanedResources(List<Hyperlink> links)
        {
            List<string> unusedResources = new ();
            List<MarkdownError> errors = new ();

            List<Hyperlink> usedResources = links
                .Where(x => x.LinkType == HyperlinkType.Resource)
                .ToList();

            // get all resources in the configure resource folder names
            string root = Path.GetFullPath(_config.DocumentationFiles.SourceFolder);
            Matcher matcher = new ();
            matcher.AddExcludePatterns(new[] { "*.md", ".*" });
            foreach (string folderName in _config.ResourceFolderNames)
            {
                matcher.AddInclude($"**/{folderName}/**.*");
            }

            IEnumerable<string> resources = matcher.GetResultsInFullPath(root);

            _console.Verbose($"Traversing {resources.Count()} resources in {root}");
            foreach (string resource in resources)
            {
                try
                {
                    _console.Verbose($"Validating {FileHelper.GetRelativePath(resource, root)}.");
                    string resourceFullPath = Path.GetFullPath(resource);
                    if (usedResources.FirstOrDefault(x => string.Compare(x.UrlFullPath, resourceFullPath, true) == 0) == null)
                    {
                        unusedResources.Add(resourceFullPath);
                        errors.Add(
                            new MarkdownError(
                                FileHelper.GetRelativePath(resourceFullPath, _config.DocumentationFiles.SourceFolder),
                                0,
                                0,
                                MarkdownErrorSeverity.Error,
                                $"Resource is not used."));
                    }
                }
                catch (Exception ex)
                {
                    _console.Error($"*** ERROR: {ex.Message}");
                }
            }

            return (unusedResources, errors);
        }

        /// <summary>
        /// Cleanup the list of provided resources.
        /// </summary>
        /// <param name="resources">List of files.</param>
        public void CleanupOrphanedResources(List<string> resources)
        {
            foreach (string resource in resources)
            {
                _console.Output($"Deleted '{FileHelper.GetRelativePath(resource, _config.DocumentationFiles.SourceFolder)}' because it was not used.");
                File.Delete(resource);
            }
        }
    }
}
