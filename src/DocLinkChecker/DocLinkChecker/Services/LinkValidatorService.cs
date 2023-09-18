namespace DocLinkChecker.Services
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Net;
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
                string whitelist = whitelistUrl;

                if (!whitelist.Contains("*") && !whitelist.Contains("?"))
                {
                    // if no wildcard is given, we'll add one to match the whole domain.
                    // this will enable to whitelist a domain without wildcards (e.g. "http://localhost").
                    whitelist += "*";
                }

                if (hyperlink.Url.Matches(whitelist))
                {
                    _console.Verbose($"Skipping whitelisted url {hyperlink.Url}");
                    return;
                }
            }

            _console.Verbose($"Validating {hyperlink.Url} in {_fileService.GetRelativePath(_config.DocumentationFiles.SourceFolder, hyperlink.FilePath)}");
            using var scope = _serviceProvider.CreateScope();
            var client = scope.ServiceProvider.GetRequiredService<CheckerHttpClient>();

            Stopwatch sw = new ();
            sw.Start();
            var result = await client.VerifyResourceSimple(hyperlink.Url);
            sw.Stop();
            if (sw.ElapsedMilliseconds > _config.DocLinkChecker.ExternalLinkDurationWarning)
            {
                _console.Warning($"*** WARNING: Checking {hyperlink.Url} took {sw.ElapsedMilliseconds}ms.");
            }

            if (!result.success)
            {
                if (result.statusCode != null)
                {
                    if ((int)result.statusCode < 300 || (int)result.statusCode > 399)
                    {
                        // we ignore redirects. Response was given, so must exist.
                        MarkdownErrorSeverity severity = MarkdownErrorSeverity.Warning;
                        if (result.statusCode == HttpStatusCode.NotFound ||
                            result.statusCode == HttpStatusCode.Gone ||
                            result.statusCode == HttpStatusCode.RequestUriTooLong)
                        {
                            // only report as error when resource isn't found or URL is too long.
                            severity = MarkdownErrorSeverity.Error;
                        }

                        _errors.Enqueue(
                            new MarkdownError(
                                hyperlink.FilePath,
                                hyperlink.Line,
                                hyperlink.Column,
                                severity,
                                $"{hyperlink.Url} => {result.statusCode}"));
                    }
                }
                else
                {
                    // no status code, but we received an error, probably an exception.
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
        /// - must be met congifured link strategy.
        /// </summary>
        /// <param name="hyperlink">Hyperlink.</param>
        /// <returns>A <see cref="Task"/> for asynchronous handling.</returns>
        private Task VerifyLocalHyperlink(Hyperlink hyperlink)
        {
            _console.Verbose($"Validating {hyperlink.Url} in {_fileService.GetRelativePath(_config.DocumentationFiles.SourceFolder, hyperlink.FilePath)}");
            string folderPath = Path.GetDirectoryName(hyperlink.FilePath);

            if (Path.IsPathRooted(hyperlink.Url))
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
            if (!_fileService.ExistsFileOrDirectory(hyperlink.UrlFullPath))
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

            switch (_config.DocLinkChecker.RelativeLinkStrategy)
            {
                case RelativeLinkType.SameDocsHierarchyOnly:
                    if (!hyperlink.UrlFullPath.StartsWith(_config.DocumentationFiles.SourceFolder))
                    {
                        _errors.Enqueue(
                            new MarkdownError(
                                hyperlink.FilePath,
                                hyperlink.Line,
                                hyperlink.Column,
                                MarkdownErrorSeverity.Error,
                                $"File referenced outside of the same /docs hierarchy not allowed: {hyperlink.Url}"));
                        return Task.CompletedTask;
                    }

                    break;

                case RelativeLinkType.AnyDocsHierarchy:
                    if (!hyperlink.UrlFullPath.Replace("\\", "/").Contains("/docs"))
                    {
                        _errors.Enqueue(
                            new MarkdownError(
                                hyperlink.FilePath,
                                hyperlink.Line,
                                hyperlink.Column,
                                MarkdownErrorSeverity.Error,
                                $"File referenced outside of anything else then a /docs hierarchy not allowed: {hyperlink.Url}"));
                        return Task.CompletedTask;
                    }

                    break;

                case RelativeLinkType.All:
                default:
                    break;
            }

            if (!string.IsNullOrEmpty(hyperlink.UrlTopic))
            {
                // validate if heading exists in file
                if (Headings
                    .FirstOrDefault(x => string.Compare(hyperlink.UrlFullPath, x.FilePath, true) == 0 &&
                                    x.Id == hyperlink.UrlTopic) == null)
                {
                    var hs = Headings
                        .Where(x => string.Compare(hyperlink.UrlFullPath, x.FilePath, true) == 0)
                        .OrderBy(x => x.Id)
                        .ToList();
                    Debug.WriteLine($"====== FOR {hyperlink.UrlFullPath} [{hyperlink.UrlTopic}] in {hyperlink.FilePath} {hyperlink.Line}:{hyperlink.Column} WE HAVE THESE OPTIONS:");
                    foreach (var h in hs)
                    {
                        Debug.WriteLine($"{h.Id}");
                    }

                    // url references a path outside of the document root
                    _errors.Enqueue(
                        new MarkdownError(
                            hyperlink.FilePath,
                            hyperlink.Line,
                            hyperlink.Column,
                            MarkdownErrorSeverity.Warning,
                            $"Heading '{hyperlink.UrlTopic}' not found in '{hyperlink.UrlWithoutTopic}'"));
                    return Task.CompletedTask;
                }
            }

            return Task.CompletedTask;
        }
    }
}
