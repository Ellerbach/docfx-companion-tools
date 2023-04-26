﻿namespace DocLinkChecker.Services
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using DocLinkChecker.Helpers;
    using DocLinkChecker.Models;
    using Markdig;
    using Markdig.Syntax;
    using Markdig.Syntax.Inlines;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// Service for validating the links in the markdown files.
    /// </summary>
    public class LinkValidatorService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly AppConfig _config;
        private readonly CustomConsoleLogger _console;

        private ManualResetEvent _quit = new ManualResetEvent(false);
        private int _timeoutMilliseconds = 500;
        private bool _noMoreInput;

        private ConcurrentQueue<string> _inputFiles = new ();
        private ConcurrentQueue<Hyperlink> _inputLinks = new ();
        private ConcurrentQueue<ValidationResult> _failed = new ();
        private List<Thread> _threads = new ();
        private object _resultsLock = new ();
        private int _successes;
        private int _failures;

        /// <summary>
        /// Initializes a new instance of the <see cref="LinkValidatorService"/> class.
        /// </summary>
        /// <param name="serviceProvider">Service provider.</param>
        /// <param name="config">App configuration.</param>
        /// <param name="console">Console logger.</param>
        public LinkValidatorService(
            IServiceProvider serviceProvider,
            AppConfig config,
            CustomConsoleLogger console)
        {
            _serviceProvider = serviceProvider;
            _config = config;
            _console = console;
            RunningTasks = StartWorkers(_config.DocLinkChecker.ConcurrencyLevel, _config.DocLinkChecker.OneHyperlinkPerThread);
        }

        /// <summary>
        /// Gets the list of running tasks.
        /// </summary>
        public Task[] RunningTasks { get; }

        /// <summary>
        /// Gets the list of failed validations.
        /// </summary>
        public IList<ValidationResult> Failed => _failed.ToList();

        /// <summary>
        /// Gets the number of successful validations.
        /// </summary>
        public int Successes
        {
            get
            {
                lock (_resultsLock)
                {
                    return _successes;
                }
            }
        }

        /// <summary>
        /// Gets the number of failed validations.
        /// </summary>
        public int Failures
        {
            get
            {
                lock (_resultsLock)
                {
                    return _failures;
                }
            }
        }

        /// <summary>
        /// Indicate that no more entries will be added to the queue to process.
        /// </summary>
        public void SignalNoMoreInput() => _noMoreInput = true;

        /// <summary>
        /// Enqueue the validation of the given filename.
        /// </summary>
        /// <param name="filename">File name.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task EnqueueFilename(string filename)
        {
            if (_config.DocLinkChecker.OneHyperlinkPerThread)
            {
                var hyperlinks = await GetLinksFromFileAsync(filename);
                foreach (var hyperlink in hyperlinks)
                {
                    if ((hyperlink.IsWeb && _config.DocLinkChecker.ValidateExternalLinks) || hyperlink.IsLocal)
                    {
                        _inputLinks.Enqueue(hyperlink);
                    }
                }
            }
            else
            {
                _inputFiles.Enqueue(filename);
            }
        }

        /// <summary>
        /// Start the workers to process the enqueued files.
        /// </summary>
        /// <param name="concurrencyLevel">Number of concurrent threads.</param>
        /// <param name="isOneHyperlinkPerThread">Value indicating whether to check 1 hyperlink per thread (true) or per file (false).</param>
        /// <returns>Workers.</returns>
        private Task[] StartWorkers(int concurrencyLevel, bool isOneHyperlinkPerThread)
        {
            List<Task> tasks = new ();
            _threads = new ();
            ParameterizedThreadStart worker = isOneHyperlinkPerThread
                ? WorkerPerLink
                : WorkerPerFile;

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

            _console.Verbose($"Started {concurrencyLevel} threads in mode {(isOneHyperlinkPerThread ? "WorkerPerLink" : "WorkerPerFile")}.");

            return tasks.ToArray();
        }

        /// <summary>
        /// Worker per file.
        /// </summary>
        /// <param name="tcsObject">Task completion source.</param>
        private async void WorkerPerFile(object tcsObject)
        {
            Debug.WriteLine($"{nameof(WorkerPerFile)} is starting on thread {Thread.CurrentThread.ManagedThreadId}");
            ArgumentNullException.ThrowIfNull(tcsObject);
            TaskCompletionSource tcs = (TaskCompletionSource)tcsObject;
            int successes = 0;
            int failures = 0;
            try
            {
                while (!_quit.WaitOne(_timeoutMilliseconds))
                {
                    if (_noMoreInput && _inputFiles.Count == 0)
                    {
                        return;
                    }

                    do
                    {
                        if (!_inputFiles.TryDequeue(out string filename))
                        {
                            break;
                        }

                        var hyperlinks = await GetLinksFromFileAsync(filename);
                        _console.Verbose($">{filename}: {hyperlinks.Count} links");
                        Debug.WriteLine($">{filename}:{hyperlinks.Count} - TID:{Thread.CurrentThread.ManagedThreadId}");
                        var (success, failure) = await VerifyHyperlinks(hyperlinks);
                        successes += success;
                        failures += failure;
                    }
                    while (!_quit.WaitOne(0));
                }
            }
            finally
            {
                Debug.WriteLine($"{nameof(WorkerPerFile)} is stopping");
                tcs.SetResult();
                lock (_resultsLock)
                {
                    _successes += successes;
                    _failures += failures;
                }
            }
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
            int successes = 0;
            int failures = 0;
            try
            {
                while (!_quit.WaitOne(_timeoutMilliseconds))
                {
                    if (_noMoreInput && _inputLinks.Count == 0)
                    {
                        return;
                    }

                    do
                    {
                        if (!_inputLinks.TryDequeue(out Hyperlink hyperlink))
                        {
                            break;
                        }

                        Debug.WriteLine($">{hyperlink.Url} - TID:{Thread.CurrentThread.ManagedThreadId}");
                        var (success, failure) = await VerifyHyperlink(hyperlink);
                        successes += success;
                        failures += failure;
                    }
                    while (!_quit.WaitOne(0));
                }
            }
            finally
            {
                Debug.WriteLine($"{nameof(WorkerPerLink)} is stopping");
                tcs.SetResult();
                lock (_resultsLock)
                {
                    _successes += successes;
                    _failures += failures;
                }
            }
        }

        /// <summary>
        /// Verify the given hyperlink.
        /// </summary>
        /// <param name="hyperlink">Hyperlink.</param>
        /// <returns>Result of verification.</returns>
        private Task<(int success, int fail)> VerifyHyperlink(Hyperlink hyperlink)
        {
            if (hyperlink.IsWeb)
            {
                return VerifyWebHyperlink(hyperlink);
            }

            return VerifyLocalHyperlink(hyperlink);
        }

        /// <summary>
        /// Verify the provided hyperlinks.
        /// </summary>
        /// <param name="hyperlinks">List of hyperlinks.</param>
        /// <returns>Number successful, number failed.</returns>
        private async Task<(int success, int fail)> VerifyHyperlinks(List<Hyperlink> hyperlinks)
        {
            var taskFiles = VerifyLocalHyperlink(hyperlinks.Where(x => x.IsLocal));
            var taskLinks = VerifyWebHyperlinks(hyperlinks.Where(x => x.IsWeb));
            await Task.WhenAll(taskFiles, taskLinks);
            var successes = taskFiles.Result.success + taskLinks.Result.success;
            var failures = taskFiles.Result.fail + taskLinks.Result.fail;
            return (successes, failures);
        }

        /// <summary>
        /// Verify the provided web hyperlinks.
        /// </summary>
        /// <param name="hyperlinks">List of web hyperlinks.</param>
        /// <returns>Number successful, number failed.</returns>
        private async Task<(int success, int fail)> VerifyWebHyperlinks(IEnumerable<Hyperlink> hyperlinks)
        {
            var successes = 0;
            var failures = 0;

            using var scope = _serviceProvider.CreateScope();
            var client = scope.ServiceProvider.GetRequiredService<CheckerHttpClient>();
            foreach (var hyperlink in hyperlinks)
            {
                foreach (string whitelistUrl in _config.DocLinkChecker.WhitelistUrls)
                {
                    if (hyperlink.Url.StartsWith(whitelistUrl))
                    {
                        break;
                    }
                }

                Debug.WriteLine($"{nameof(VerifyWebHyperlinks)} {hyperlink.Url}");
                var (success, statusCode, error) = await client.VerifyResource(hyperlink.Url);
                if (!success)
                {
                    _console.Error($"ERROR:\n{hyperlink.FullPathName}:{hyperlink.LineNum}\nWeb link {hyperlink.Url} error {statusCode}");
                    var validationResult = new ValidationResult(success, statusCode, error, hyperlink);
                    _failed.Enqueue(validationResult);
                    failures++;
                }
                else
                {
                    successes++;
                }
            }

            return (successes, failures);
        }

        /// <summary>
        /// Verify the given list of hyperlinks.
        /// </summary>
        /// <param name="hyperlinks">Hyperlinks.</param>
        /// <returns>Result of verification.</returns>
        private async Task<(int success, int fail)> VerifyLocalHyperlink(IEnumerable<Hyperlink> hyperlinks)
        {
            int successes = 0;
            int failures = 0;
            foreach (var hyperlink in hyperlinks)
            {
                var (success, failure) = await VerifyLocalHyperlink(hyperlink);
                successes += success;
                failures += failure;
            }

            return (successes, failures);
        }

        /// <summary>
        /// Verify provided web hyperlink.
        /// </summary>
        /// <param name="hyperlink">Hyperlink to validate.</param>
        /// <returns>Number successful, number failed.</returns>
        private async Task<(int success, int fail)> VerifyWebHyperlink(Hyperlink hyperlink)
        {
            var successes = 0;
            var failures = 0;

            foreach (string whitelistUrl in _config.DocLinkChecker.WhitelistUrls)
            {
                if (hyperlink.Url.StartsWith(whitelistUrl))
                {
                    return (0, 0);
                }
            }

            _console.Verbose($">{hyperlink.Url}");
            using var scope = _serviceProvider.CreateScope();
            var client = scope.ServiceProvider.GetRequiredService<CheckerHttpClient>();
            var (success, statusCode, error) = await client.VerifyResource(hyperlink.Url);
            if (!success)
            {
                // string statusCodeName = Enum.GetName(typeof(HttpStatusCode), statusCode);
                _console.Error($"*** ERROR ***\n{hyperlink.FullPathName}:{hyperlink.LineNum}\n{hyperlink.Url}\n{statusCode}");
                var validationResult = new ValidationResult(success, statusCode, error, hyperlink);
                _failed.Enqueue(validationResult);
                failures++;
            }
            else
            {
                successes++;
            }

            return (successes, failures);
        }

        /// <summary>
        /// Verify local hyperlink.
        /// - must exist.
        /// - must be relative.
        /// - must be within document folder hierarchy (except for config.AllowLinksOutsideHierarchy = true).
        /// </summary>
        /// <param name="hyperlink">Hyperlink.</param>
        /// <returns>Result of verification.</returns>
        private Task<(int success, int fail)> VerifyLocalHyperlink(Hyperlink hyperlink)
        {
            _console.Verbose($">{hyperlink.Url}");
            string urlFullPath = Path.GetFullPath(hyperlink.Url);

            if (!File.Exists(urlFullPath))
            {
                // referenced file doesn't exist
                _console.Error($"*** ERROR ***\n{hyperlink.FullPathName}:{hyperlink.LineNum}\nNot found: {hyperlink.Url}");
                var validationResult = new ValidationResult(false, null, "Link does not exists", hyperlink);
                _failed.Enqueue(validationResult);
                return Task.FromResult((0, 1));
            }

            if (string.Compare(hyperlink.Url.TrimEnd(Path.DirectorySeparatorChar), urlFullPath.TrimEnd(Path.DirectorySeparatorChar), true) == 0)
            {
                // url equals fullpath. We don't allow full local path references
                _console.Error($"*** ERROR ***\n{hyperlink.FullPathName}:{hyperlink.LineNum}\nFull path not allowed: {hyperlink.Url}");
                var validationResult = new ValidationResult(false, null, "Full path not allowed", hyperlink);
                _failed.Enqueue(validationResult);
                return Task.FromResult((0, 1));
            }

            if (!urlFullPath.StartsWith(_config.DocumentsRoot))
            {
                // url references a path outside of the document root
                _console.Error($"*** ERROR ***\n{hyperlink.FullPathName}:{hyperlink.LineNum}\nFile referenced outside of the docs hierarchy '{_config.DocumentsRoot}': {hyperlink.Url}");
                var validationResult = new ValidationResult(false, null, $"File referenced outside of the docs hierarchy '{_config.DocumentsRoot}'", hyperlink);
                _failed.Enqueue(validationResult);
                return Task.FromResult((0, 1));
            }

            return Task.FromResult((1, 0));
        }

        /// <summary>
        /// Get the links from the given file.
        /// </summary>
        /// <param name="filename">File to read.</param>
        /// <returns>List of hyperlinks.</returns>
        private async Task<List<Hyperlink>> GetLinksFromFileAsync(string filename)
        {
            string fullPathname = null;
            try
            {
                fullPathname = Path.GetFullPath(filename);
                var md = await File.ReadAllTextAsync(fullPathname);
                return GetLinksFromMarkdownString(fullPathname, md);
            }
            catch (Exception err)
            {
                _console.Error($"ERROR: reading {fullPathname}. {err.Message}");
                return new List<Hyperlink>()
                {
                    new Hyperlink(fullPathname ?? filename, 0, string.Empty),
                };
            }
        }

        /// <summary>
        /// Get all links from the given markdown string.
        /// </summary>
        /// <param name="fullPathname">Full path name where markdown is taken from.</param>
        /// <param name="markdown">Markdown content.</param>
        /// <returns>List of hyperlinks.</returns>
        private List<Hyperlink> GetLinksFromMarkdownString(string fullPathname, string markdown)
        {
            try
            {
                var pipeline = new MarkdownPipelineBuilder()
                    .UseAdvancedExtensions()
                    .Build();
                var document = Markdown.Parse(markdown, pipeline);

                var links = document
                    .Descendants<LinkInline>()
                    .Select(d => new Hyperlink(fullPathname, d.Line, d.Url ?? string.Empty))
                    .ToList();

                return links ?? new List<Hyperlink>();
            }
            catch (Exception err)
            {
                _console.Error($"ERROR: parsing markdown from {fullPathname}. {err.Message}");
                return new List<Hyperlink>()
                {
                    new Hyperlink(fullPathname, 0, string.Empty),
                };
            }
        }
    }
}