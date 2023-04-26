namespace DocLinkChecker
{
    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using DocLinkChecker.Services;
    using Microsoft.Extensions.Hosting;

    /// <summary>
    /// The main logic of the app.
    /// </summary>
    public class App : BackgroundService
    {
        private readonly CrawlerService _crawler;
        private readonly LinkValidatorService _checker;
        private readonly CustomConsoleLogger _console;
        private readonly IHostApplicationLifetime _hostApplicationLifetime;

        /// <summary>
        /// Initializes a new instance of the <see cref="App"/> class.
        /// </summary>
        /// <param name="crawler">Crawler service.</param>
        /// <param name="checker">Checker service.</param>
        /// <param name="console">Console logger.</param>
        /// <param name="hostApplicationLifetime">Host application lifetime.</param>
        public App(
            CrawlerService crawler,
            LinkValidatorService checker,
            CustomConsoleLogger console,
            IHostApplicationLifetime hostApplicationLifetime)
        {
            _crawler = crawler;
            _checker = checker;
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
            _console.Verbose($"DocLinkChecker started at {DateTime.Now}");
            Stopwatch sw = new ();
            sw.Start();
            int processed = await _crawler.WalkTreeForMarkdown(_checker.EnqueueFilename);

            _checker.SignalNoMoreInput();
            await Task.WhenAll(_checker.RunningTasks);
            sw.Stop();

            // _console.Error(string.Join(Environment.NewLine, _checker.Failed.Select(f => $"{f.ToReport()}")));
            _console.Verbose($"Total Time: {sw.Elapsed.TotalSeconds}s");
            _console.Output($"Links - Succeeded:{_checker.Successes}; Failed:{_checker.Failures}");
            _console.Output($"Files - Processed:{processed}");

            if (_checker.Failures > 0)
            {
                Program.ReturnValue = Enums.ReturnValue.ProcessingErrors;
            }

            _hostApplicationLifetime.StopApplication();
        }
    }
}
