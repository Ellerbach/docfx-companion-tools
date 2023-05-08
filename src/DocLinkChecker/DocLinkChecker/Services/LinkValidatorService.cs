namespace DocLinkChecker.Services
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using DocLinkChecker.Enums;
    using DocLinkChecker.Helpers;
    using DocLinkChecker.Interfaces;
    using DocLinkChecker.Models;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// Service for validating the links in the markdown files.
    /// </summary>
    public class LinkValidatorService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly AppConfig _config;
        private readonly IFileService _fileService;
        private readonly ICustomConsoleLogger _console;

        private ManualResetEvent _quit = new ManualResetEvent(false);
        private int _timeoutMilliseconds = 500;
        private bool _noMoreInput;

        private ConcurrentQueue<Hyperlink> _inputLinks = new ();
        private ConcurrentQueue<MarkdownError> _errors = new ();
        private List<Thread> _threads = new ();
        private object _resultsLock = new ();

        /// <summary>
        /// Initializes a new instance of the <see cref="LinkValidatorService"/> class.
        /// </summary>
        /// <param name="serviceProvider">Service provider.</param>
        /// <param name="config">App configuration.</param>
        /// <param name="fileService">File service.</param>
        /// <param name="console">Console logger.</param>
        public LinkValidatorService(
            IServiceProvider serviceProvider,
            AppConfig config,
            IFileService fileService,
            ICustomConsoleLogger console)
        {
            _serviceProvider = serviceProvider;
            _config = config;
            _fileService = fileService;
            _console = console;
            RunningTasks = StartWorkers(_config.DocLinkChecker.ConcurrencyLevel);
        }

        /// <summary>
        /// Gets the list of headings.
        /// </summary>
        public List<Heading> Headings { get; } = new ();

        /// <summary>
        /// Gets the list of running tasks.
        /// </summary>
        public Task[] RunningTasks { get; }

        /// <summary>
        /// Gets the list of failed validations.
        /// </summary>
        public List<MarkdownError> Errors => _errors.ToList();

        /// <summary>
        /// Indicate that no more entries will be added to the queue to process.
        /// </summary>
        public void SignalNoMoreInput() => _noMoreInput = true;

        /// <summary>
        /// Enqueue the validation of the given link.
        /// </summary>
        /// <param name="link">Hyperlink to validate.</param>
        public void EnqueueLinkForValidation(Hyperlink link)
        {
            _inputLinks.Enqueue(link);
        }

        /// <summary>
        /// Verify the given hyperlink.
        /// </summary>
        /// <param name="hyperlink">Hyperlink.</param>
        /// <returns>A <see cref="Task"/> for asynchronous handling.</returns>
        public Task VerifyHyperlink(Hyperlink hyperlink)
        {
            if (hyperlink.IsWeb)
            {
                return VerifyWebHyperlink(hyperlink);
            }

            if (hyperlink.IsLocal)
            {
                return VerifyLocalHyperlink(hyperlink);
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Start the workers to process the enqueued files.
        /// </summary>
        /// <param name="concurrencyLevel">Number of concurrent threads.</param>
        /// <returns>Workers.</returns>
        private Task[] StartWorkers(int concurrencyLevel)
        {
            List<Task> tasks = new ();
            _threads = new ();
            ParameterizedThreadStart worker = WorkerPerLink;

            for (int i = 0; i < concurrencyLevel; i++)
            {
                TaskCompletionSource tcs = new TaskCompletionSource();
                var thread = new Thread(worker);
                _threads.Add(thread);
                tasks.Add(tcs.Task);
                thread.IsBackground = true;
                thread.Priority = ThreadPriority.Normal;
                thread.Start(tcs);
            }

            _console.Verbose($"Started {concurrencyLevel} threads for link validation.");

            return tasks.ToArray();
        }

        /// <summary>
        /// Worker per link.
        /// </summary>
        /// <param name="tcsObject">Task completion source.</param>
        private async void WorkerPerLink(object tcsObject)
        {
            Debug.WriteLine($"{nameof(WorkerPerLink)} is starting on thread {Thread.CurrentThread.ManagedThreadId}");
            ArgumentNullException.ThrowIfNull(tcsObject);
            TaskCompletionSource tcs = (TaskCompletionSource)tcsObject;
            try
            {
                while (!_quit.WaitOne(_timeoutMilliseconds))
                {
                    if (_noMoreInput && _inputLinks.Count == 0)
                    {
                        // nothing more to check, so quit.
                        return;
                    }

                    do
                    {
                        if (!_inputLinks.TryDequeue(out Hyperlink hyperlink))
                        {
                            break;
                        }

                        Debug.WriteLine($">{hyperlink.Url} - TID:{Thread.CurrentThread.ManagedThreadId}");
                        await VerifyHyperlink(hyperlink);
                    }
                    while (!_quit.WaitOne(0));
                }
            }
            finally
            {
                Debug.WriteLine($"{nameof(WorkerPerLink)} on thread {Thread.CurrentThread.ManagedThreadId} is stopping");
                tcs.SetResult();
            }
        }

        /// <summary>
        /// Verify provided web hyperlink.
        /// </summary>
        /// <param name="hyperlink">Hyperlink to validate.</param>
        /// <returns>A <see cref="Task"/> for asynchronous handling.</returns>
        private async Task VerifyWebHyperlink(Hyperlink hyperlink)
        {
            foreach (string whitelistUrl in _config.DocLinkChecker.WhitelistUrls)
            {
                if (hyperlink.Url.StartsWith(whitelistUrl))
                {
                    _console.Verbose($"Skipping whitelisted url {hyperlink.Url}");
                    return;
                }
            }

            _console.Verbose($"Validating {hyperlink.Url} in {_fileService.GetRelativePath(hyperlink.FilePath, _config.DocumentationFiles.SourceFolder)}");
            using var scope = _serviceProvider.CreateScope();
            var client = scope.ServiceProvider.GetRequiredService<CheckerHttpClient>();

            Stopwatch sw = new ();
            sw.Start();
            var result = await client.VerifyResource(hyperlink.Url);
            sw.Stop();
            if (sw.ElapsedMilliseconds > 3000)
            {
                _console.Warning($"*** WARNING: Checking {hyperlink.Url} took {sw.ElapsedMilliseconds}ms.");
            }

            if (!result.success)
            {
                if (result.statusCode != null)
                {
                    _errors.Enqueue(
                        new MarkdownError(
                            hyperlink.FilePath,
                            hyperlink.Line,
                            hyperlink.Column,
                            MarkdownErrorSeverity.Error,
                            $"{hyperlink.Url} => {result.statusCode}"));
                }
                else
                {
                    _errors.Enqueue(
                        new MarkdownError(
                            hyperlink.FilePath,
                            hyperlink.Line,
                            hyperlink.Column,
                            MarkdownErrorSeverity.Error,
                            $"{hyperlink.Url} => {result.error}"));
                }
            }
        }

        /// <summary>
        /// Verify local hyperlink.
        /// - must exist.
        /// - must be relative.
        /// - must be within document folder hierarchy (except for config.AllowLinksOutsideHierarchy = true).
        /// </summary>
        /// <param name="hyperlink">Hyperlink.</param>
        /// <returns>A <see cref="Task"/> for asynchronous handling.</returns>
        private Task VerifyLocalHyperlink(Hyperlink hyperlink)
        {
            _console.Verbose($"Validating {hyperlink.Url} in {_fileService.GetRelativePath(hyperlink.FilePath, _config.DocumentationFiles.SourceFolder)}");
            string folderPath = Path.GetDirectoryName(hyperlink.FilePath);
            var parts = GetLinkDetails(folderPath, hyperlink.Url);

            if (string.Compare(hyperlink.Url.TrimEnd(Path.DirectorySeparatorChar), parts.fullpath, true) == 0)
            {
                // url equals fullpath. We don't allow full local path references
                _errors.Enqueue(
                    new MarkdownError(
                        hyperlink.FilePath,
                        hyperlink.Line,
                        hyperlink.Column,
                        MarkdownErrorSeverity.Error,
                        $"Full path not allowed as link: {hyperlink.Url}"));
                return Task.CompletedTask;
            }

            // compute link of the url relative to the path of the file.
            if (!_fileService.ExistsFileOrDirectory(parts.fullpath))
            {
                // referenced file doesn't exist
                _errors.Enqueue(
                    new MarkdownError(
                        hyperlink.FilePath,
                        hyperlink.Line,
                        hyperlink.Column,
                        MarkdownErrorSeverity.Error,
                        $"Not found: {hyperlink.Url}"));
                return Task.CompletedTask;
            }

            if (!parts.fullpath.StartsWith(_config.DocumentationFiles.SourceFolder) &&
                !_config.DocLinkChecker.AllowResourcesOutsideDocumentsRoot)
            {
                // url references a path outside of the document root
                _errors.Enqueue(
                    new MarkdownError(
                        hyperlink.FilePath,
                        hyperlink.Line,
                        hyperlink.Column,
                        MarkdownErrorSeverity.Error,
                        $"File referenced outside of the docs hierarchy not allowed: {hyperlink.Url}"));
                return Task.CompletedTask;
            }

            if (!string.IsNullOrEmpty(parts.heading))
            {
                // validate if heading exists in file
                if (Headings.FirstOrDefault(x => string.Compare(parts.fullpath, x.FilePath, true) == 0 && x.Id == parts.heading) == null)
                {
                    // url references a path outside of the document root
                    _errors.Enqueue(
                        new MarkdownError(
                            hyperlink.FilePath,
                            hyperlink.Line,
                            hyperlink.Column,
                            MarkdownErrorSeverity.Error,
                            $"Heading '{parts.heading}' not found '{parts.path}'"));
                    return Task.CompletedTask;
                }
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Get the link and the heading-id if available.
        /// </summary>
        /// <param name="folderPath">Folder path.</param>
        /// <param name="url">Url.</param>
        /// <returns>Relative path, Full path and heading.</returns>
        private (string path, string fullpath, string heading) GetLinkDetails(string folderPath, string url)
        {
            string path = url;
            string fullpath = _fileService.GetFullPath(Path.Combine(folderPath, url)).TrimEnd(Path.DirectorySeparatorChar);
            string topic = string.Empty;
            if (fullpath.Contains("#"))
            {
                int pos = fullpath.IndexOf("#");
                topic = fullpath.Substring(pos + 1);
                fullpath = fullpath.Substring(0, pos);

                pos = path.IndexOf("#");
                path = path.Substring(0, pos);
            }

            return (path, fullpath, topic);
        }
    }
}
