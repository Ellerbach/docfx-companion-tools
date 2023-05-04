namespace DocLinkChecker.Services
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using DocLinkChecker.Constants;
    using DocLinkChecker.Helpers;
    using DocLinkChecker.Models;
    using Microsoft.Extensions.FileSystemGlobbing;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Crawler service implementation.
    /// </summary>
    public class CrawlerService
    {
        private readonly AppConfig _config;
        private readonly CustomConsoleLogger _console;

        /// <summary>
        /// Initializes a new instance of the <see cref="CrawlerService"/> class.
        /// </summary>
        /// <param name="config">App configuration.</param>
        /// <param name="console">Console logger.</param>
        /// <param name="logger">Logger.</param>
        public CrawlerService(
            AppConfig config,
            CustomConsoleLogger console,
            ILogger<CrawlerService> logger)
        {
            _config = config;
            _console = console;
        }

        /// <summary>
        /// Get all markdownfiles in the documents root and it's subfolders and parse them.
        /// </summary>
        /// <returns>List of hyperlinks and list of errors (which can be empty).</returns>
        public async Task<(List<MarkdownObjectBase> objects, List<MarkdownError> errors)> ParseMarkdownFiles()
        {
            List<MarkdownObjectBase> objects = new ();
            List<MarkdownError> errors = new ();

            // get all resources in the configure resource folder names
            string root = Path.GetFullPath(_config.DocumentationFiles.SourceFolder);
            Matcher matcher = new ();
            if (_config.DocumentationFiles.Files.Any())
            {
                foreach (string folderName in _config.DocumentationFiles.Files)
                {
                    matcher.AddInclude(folderName);
                }
            }
            else
            {
                matcher.AddInclude("**/*.md");
            }

            foreach (string folderName in _config.DocumentationFiles.Exclude)
            {
                matcher.AddExclude(folderName);
            }

            IEnumerable<string> allFiles = matcher.GetResultsInFullPath(root);

            var mdFiles = allFiles.Where(x => Path.GetExtension(x).ToUpperInvariant() == ".MD");
            if (!mdFiles.Any())
            {
                _console.Warning($"***WARNING: We didn't find any markdown files in {_config.DocumentationFiles.SourceFolder} with the pattern {string.Join(',', _config.DocumentationFiles.Files)}. Please check the Files pattern or the documentation folder you configured.");
            }

            _console.Verbose($"Traversing {allFiles.Count()} files in {root}");
            foreach (var file in allFiles)
            {
                try
                {
                    if (Path.GetExtension(file).ToLowerInvariant() == ".md")
                    {
                        _console.Verbose($"Parsing markdown in '{FileHelper.GetRelativePath(file, _config.DocumentationFiles.SourceFolder)}'.");
                        var result = await MarkdownHelper.ParseMarkdownFileAsync(file, _config.DocLinkChecker.ValidatePipeTableFormatting);
                        objects.AddRange(result.objects);
                        errors.AddRange(result.errors);
                    }
                }
                catch (Exception ex)
                {
                    _console.Error($"*** ERROR: {ex.Message}");
                }
            }

            return (objects, errors);
        }

        /// <summary>
        /// Parse the list and call the delegate for each item.
        /// </summary>
        /// <param name="objects">List of (filtered) markdown objects.</param>
        /// <param name="handler">Delegate to execute for each entry.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task ExecuteForObjects(List<MarkdownObjectBase> objects, Func<MarkdownObjectBase, Task> handler)
        {
            foreach (var obj in objects)
            {
                await handler(obj);
            }
        }
    }
}
