namespace DocLinkChecker
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using DocLinkChecker.Enums;
    using DocLinkChecker.Models;
    using DocLinkChecker.Services;
    using Microsoft.Extensions.Hosting;

    /// <summary>
    /// The main logic of the app.
    /// </summary>
    public class App : BackgroundService
    {
        private readonly AppConfig _config;
        private readonly CrawlerService _crawler;
        private readonly LinkValidatorService _linkValidator;
        private readonly ResourceValidatorService _resourceValidator;
        private readonly CustomConsoleLogger _console;
        private readonly IHostApplicationLifetime _hostApplicationLifetime;

        /// <summary>
        /// Initializes a new instance of the <see cref="App"/> class.
        /// </summary>
        /// <param name="config">Application configuration.</param>
        /// <param name="crawler">Crawler service.</param>
        /// <param name="linkValidator">Link validator service.</param>
        /// <param name="resourceValidator">Resource validator service.</param>
        /// <param name="console">Console logger.</param>
        /// <param name="hostApplicationLifetime">Host application lifetime.</param>
        public App(
            AppConfig config,
            CrawlerService crawler,
            LinkValidatorService linkValidator,
            ResourceValidatorService resourceValidator,
            CustomConsoleLogger console,
            IHostApplicationLifetime hostApplicationLifetime)
        {
            _config = config;
            _crawler = crawler;
            _resourceValidator = resourceValidator;
            _linkValidator = linkValidator;
            _console = console;
            _hostApplicationLifetime = hostApplicationLifetime;
        }

        /// <inheritdoc/>
        public override Task StopAsync(CancellationToken cancellationToken)
        {
            var baseres = base.StopAsync(cancellationToken);
            _hostApplicationLifetime.StopApplication();
            return baseres;
        }

        /// <inheritdoc/>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _console.Output($"DocLinkChecker version {Assembly.GetEntryAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion} started at {DateTime.Now}");
            Stopwatch sw = new ();
            sw.Start();

            try
            {
                List<MarkdownError> errors = new ();

                // get all information from markdown files
                var parsed = await _crawler.ParseMarkdownFiles();
                errors.AddRange(parsed.errors);

                // add the headings to the _checker to validate heading references
                var headings = parsed.objects.OfType<Heading>().ToList();
                _linkValidator.Headings.AddRange(headings);

                // validate the links
                List<Hyperlink> links = parsed.objects.OfType<Hyperlink>().ToList();
                foreach (Hyperlink link in links)
                {
                    _linkValidator.EnqueueLinkForValidation(link);
                }

                _linkValidator.SignalNoMoreInput();
                await Task.WhenAll(_linkValidator.RunningTasks);
                sw.Stop();

                // add link errors to the list of errors
                errors.AddRange(_linkValidator.Errors);

                // if configured, check for orphaned resources
                if (_config.DocLinkChecker.CheckForOrphanedResources)
                {
                    var resourceValidationResult = _resourceValidator.CheckForOrphanedResources(links);
                    errors.AddRange(resourceValidationResult.errors);
                }

                // sort the errors on file and file position and show them
                errors = errors.OrderBy(x => x.MarkdownFilePath).ThenBy(x => x.Line).ThenBy(x => x.Column).ToList();
                if (errors.Any())
                {
                    Program.ReturnValue = ReturnValue.ProcessingErrors;
                }

                foreach (MarkdownError error in errors)
                {
                    switch (error.Severity)
                    {
                        case MarkdownErrorSeverity.Error:
                            _console.Output(error.GetLocationString(_config.DocumentationFiles.SourceFolder));
                            _console.Error($"***{error.Severity.ToString().ToUpperInvariant()} {error.Message}\n");
                            break;
                        case MarkdownErrorSeverity.Warning:
                            _console.Output(error.GetLocationString(_config.DocumentationFiles.SourceFolder));
                            _console.Warning($"***{error.Severity.ToString().ToUpperInvariant()} {error.Message}\n");
                            break;
                        default:
                            _console.Verbose(error.GetLocationString(_config.DocumentationFiles.SourceFolder));
                            _console.Verbose($"***{error.Severity.ToString().ToUpperInvariant()} {error.Message}\n");
                            break;
                    }
                }

                _console.Verbose($"Total Time: {sw.Elapsed.TotalSeconds}s");
            }
            catch (Exception ex)
            {
                _console.Error($"***EXCEPTION: {ex.Message}");
            }
            finally
            {
                _hostApplicationLifetime.StopApplication();
            }
        }
    }
}
