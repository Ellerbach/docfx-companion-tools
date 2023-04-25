namespace DocLinkChecker.Services
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using DocLinkChecker.Constants;
    using DocLinkChecker.Models;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Crawler service implementation.
    /// </summary>
    public class CrawlerService
    {
        private readonly AppConfig _config;
        private readonly CustomConsoleLogger _console;
        private readonly ILogger<CrawlerService> _logger;

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
            _logger = logger;
        }

        /// <summary>
        /// Walk the folder tree for markdown files in the given documents folder.
        /// </summary>
        /// <param name="onFile">Delegate to execute for each file.</param>
        /// <returns>Number processed.</returns>
        public async Task<int> WalkTreeForMarkdown(Func<string, Task> onFile)
        {
            int processedCounter = 0;
            var root = Path.GetFullPath(_config.DocumentsFolder);
            var allFiles = Directory.EnumerateFiles(root, $"*.{AppConstants.MarkdownExtension}", SearchOption.AllDirectories);
            _console.Verbose($"Traversing {allFiles.Count()} files in {root}");
            foreach (var file in allFiles)
            {
                FileInfo fi = new (file);
                if (fi.DirectoryName == null)
                {
                    _logger.LogError($"There is no DirectoryName for {file}");
                    continue;
                }

                await onFile(file);
                processedCounter++;
            }

            return processedCounter;
        }

        /// <summary>
        /// Walk the folder tree for resource files.
        /// </summary>
        /// <param name="onFile">Delegate to execute for each file.</param>
        /// <returns>Number processed.</returns>
        public async Task<int> WalkTreeForResources(Func<string, Task> onFile)
        {
            int processedCounter = 0;
            var root = Path.GetFullPath(_config.DocumentsFolder);
            var allFiles = Directory.EnumerateFiles(root, $"*.{AppConstants.MarkdownExtension}", SearchOption.AllDirectories);
            _console.Verbose($"Traversing {allFiles.Count()} files in {root}");
            foreach (var file in allFiles)
            {
                FileInfo fi = new (file);
                if (fi.DirectoryName == null)
                {
                    _logger.LogError($"There is no DirectoryName for {file}");
                    continue;
                }

                await onFile(file);
                processedCounter++;
            }

            return processedCounter;
        }
    }
}
