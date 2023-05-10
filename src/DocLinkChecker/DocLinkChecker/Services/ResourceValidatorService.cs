namespace DocLinkChecker.Services
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using DocLinkChecker.Enums;
    using DocLinkChecker.Interfaces;
    using DocLinkChecker.Models;

    /// <summary>
    /// Resource validation service.
    /// </summary>
    public class ResourceValidatorService
    {
        private readonly AppConfig _config;
        private readonly IFileService _fileService;
        private readonly ICustomConsoleLogger _console;

        /// <summary>
        /// Initializes a new instance of the <see cref="ResourceValidatorService"/> class.
        /// </summary>
        /// <param name="config">App configuration.</param>
        /// <param name="fileService">File service.</param>
        /// <param name="console">Console logger.</param>
        public ResourceValidatorService(
            AppConfig config,
            IFileService fileService,
            ICustomConsoleLogger console)
        {
            _config = config;
            _fileService = fileService;
            _console = console;
        }

        /// <summary>
        /// Check for orphaned resources.
        /// We check all folders with the name(s) specified in the configuration, including the sub folders.
        /// If files are stored there that are not referenced in the markdown files, they are considered orphaned.
        /// </summary>
        /// <param name="links">Used links.</param>
        /// <returns>List of unused resources and a list of errors.</returns>
        public (List<string> orphanedResources, List<MarkdownError> errors) CheckForOrphanedResources(List<Hyperlink> links)
        {
            List<string> orphanedResources = new ();
            List<MarkdownError> errors = new ();

            List<Hyperlink> usedResources = links
                .Where(x => x.LinkType == HyperlinkType.Resource)
                .ToList();

            // get all resources in the configure resource folder names
            string root = _fileService.GetFullPath(_config.DocumentationFiles.SourceFolder);
            List<string> includes = new ();
            foreach (string folderName in _config.ResourceFolderNames)
            {
                includes.Add($"**/{folderName}/**.*");
            }

            IEnumerable<string> resources = _fileService.GetFiles(root, includes, new () { "*.md", ".*" });

            _console.Verbose($"Traversing {resources.Count()} resources in {root}");
            foreach (string resource in resources)
            {
                try
                {
                    _console.Verbose($"Validating {_fileService.GetRelativePath(root, resource)}.");
                    string resourceFullPath = Path.GetFullPath(resource);
                    if (usedResources.FirstOrDefault(x => string.Compare(x.UrlFullPath, resourceFullPath, true) == 0) == null)
                    {
                        orphanedResources.Add(resourceFullPath);
                        errors.Add(
                            new MarkdownError(
                                _fileService.GetRelativePath(resourceFullPath, _config.DocumentationFiles.SourceFolder),
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

            return (orphanedResources, errors);
        }

        /// <summary>
        /// Cleanup the list of provided resources.
        /// </summary>
        /// <param name="resources">List of files.</param>
        public void CleanupOrphanedResources(List<string> resources)
        {
            foreach (string resource in resources)
            {
                _console.Output($"Deleted '{_fileService.GetRelativePath(_config.DocumentationFiles.SourceFolder, resource)}' because it was not used.");
                _fileService.DeleteFile(resource);
            }
        }
    }
}
