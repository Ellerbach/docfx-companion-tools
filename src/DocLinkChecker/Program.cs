// Licensed to DocFX Companion Tools and contributors under one or more agreements.
// DocFX Companion Tools and contributors licenses this file to you under the MIT license.
namespace DocLinkChecker
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Text;
    using System.Text.Json;
    using CommandLine;
    using DocLinkChecker.Constants;
    using DocLinkChecker.Enums;
    using DocLinkChecker.Models;
    using DocLinkChecker.Services;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Polly;
    using Polly.Extensions.Http;
    using Polly.Timeout;

    /// <summary>
    /// Main program class for documentation link checker tool. It's a command-line tool
    /// that takes parameters. Use -help as parameter to see the syntax.
    /// </summary>
    public class Program
    {
        private static AppConfig _appConfig = new ();

        /// <summary>
        /// Gets or sets the return value of the application.
        /// </summary>
        public static ReturnValue ReturnValue { get; set; } = ReturnValue.Processing;

        /// <summary>
        /// Main entry point of the application.
        /// </summary>
        /// <param name="args">Commandline parameters.</param>
        /// <returns>Result of the process.</returns>
        public static int Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            CreateHostBuilder(args).Build().Run();
            return (int)ReturnValue;
        }

        /// <summary>
        /// Configure the application.
        /// </summary>
        /// <param name="args">Commandline parameters.</param>
        /// <returns><see cref="IHostBuilder"/>.</returns>
        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            DetermineConfiguration(args);
            if (ReturnValue != ReturnValue.Processing)
            {
                Environment.Exit((int)ReturnValue);
            }

            return Host.CreateDefaultBuilder(args)
                //// Ctrl-C
                .UseConsoleLifetime()
                .ConfigureLogging(options =>
                {
                    // Microsoft.Extensions.Logging
                    options.ClearProviders();
                    options.AddDebug();
                })
                .ConfigureAppConfiguration(config =>
                {
                })
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddSingleton<AppConfig>(_appConfig);
                    services.AddSingleton<CustomConsoleLogger>(new CustomConsoleLogger(_appConfig.Verbose));
                    services.AddSingleton<CrawlerService>();
                    services.AddSingleton<LinkValidatorService>();

                    services.AddHttpClient(AppConstants.HttpClientName, c =>
                    {
                        // NOTE: we commented the next line. It can cause errors from servers not accepting all user-agents.
                        // c.DefaultRequestHeaders.Add("User-Agent", "DocLinkChecker Http Client");
                        c.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*"));
                        c.DefaultRequestHeaders.ConnectionClose = true;
                        c.DefaultRequestHeaders.CacheControl = new CacheControlHeaderValue
                        {
                            NoCache = true,
                            NoStore = true,
                            MaxAge = new TimeSpan(0),
                            MustRevalidate = true,
                        };
                        c.Timeout = TimeSpan.FromSeconds(30);
                    })
                    .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
                    {
                        MaxConnectionsPerServer = 100,
                        //// HttpClient does not support redirects from https to http:
                        //// https://github.com/dotnet/runtime/issues/28039
                        AllowAutoRedirect = false,
                    })
                    .AddPolicyHandler(policy =>
                    {
                        return HttpPolicyExtensions
                            .HandleTransientHttpError()
                            .Or<TimeoutRejectedException>()
                            .WaitAndRetryAsync(3, retry => TimeSpan.FromSeconds(1));
                    })
                    .AddTypedClient<CheckerHttpClient>();

                    services.AddHostedService<App>();
                });
        }

        /// <summary>
        /// Read the configuration file and commandline parameters.
        /// Configuration is the base when available. Commandline parameters overrule settings.
        /// It also supports the 'old' way of ONLY using commandline parameters.
        /// </summary>
        /// <param name="args">Commandline parameters.</param>
        private static void DetermineConfiguration(string[] args)
        {
            CustomConsoleLogger console = new CustomConsoleLogger(true);

            // if first argument is "INIT", we'll generate a basic settings file
            // NOTE: We don't use verbs with CommandlineParser because of backwards compatability
            if (args.Length > 0 && args[0].ToUpperInvariant() == AppConstants.AppConfigInitCommand)
            {
                if (File.Exists(AppConstants.AppConfigFileName))
                {
                    console.Error($"ERROR: {AppConstants.AppConfigFileName} already exists in this folder. We don't overwrite.");

                    // indicate we're done with an error
                    ReturnValue = ReturnValue.CommandError;
                    return;
                }
                else
                {
                    string json = JsonSerializer.Serialize(_appConfig, new JsonSerializerOptions { WriteIndented = true });
                    File.WriteAllText(AppConstants.AppConfigFileName, json);
                    console.Output($"Initial configuration saved in {AppConstants.AppConfigFileName}");

                    // indicate we're done with an error
                    ReturnValue = ReturnValue.OK;
                    return;
                }
            }

            // Parse the parameters
            Parser.Default.ParseArguments<CommandLineOptions>(args)
                .WithParsed<CommandLineOptions>(ProcessCommandlineParameters)
                .WithNotParsed(HandleParameterErrors);

            console = new CustomConsoleLogger(_appConfig.Verbose);
            console.Verbose($"=== App settings ===");
            console.Verbose($"{_appConfig}");
        }

        /// <summary>
        /// Handler for all errors in commandline parsing.
        /// We indicated there are parameter errors to stop processing.
        /// </summary>
        private static void HandleParameterErrors(IEnumerable<Error> obj)
        {
            ReturnValue = ReturnValue.ParameterErrors;
        }

        /// <summary>
        /// Run the logic of the app with the given parameters.
        /// Given folders are checked if they exist.
        /// </summary>
        /// <param name="o">Parsed commandline options.</param>
        private static void ProcessCommandlineParameters(CommandLineOptions o)
        {
            CustomConsoleLogger console = new CustomConsoleLogger(true);
            _appConfig = new ();

            if (!string.IsNullOrEmpty(o.ConfigFilePath))
            {
                if (!File.Exists(o.ConfigFilePath))
                {
                    console.Error($"ERROR: configuration file {o.ConfigFilePath} not found.");

                    // indicate we're done with errors in the configuration file
                    ReturnValue = ReturnValue.ConfigurationFileErrors;
                    return;
                }

                try
                {
                    string json = File.ReadAllText(o.ConfigFilePath);
                    AppConfig parsed = JsonSerializer.Deserialize<AppConfig>(json);
                    _appConfig = parsed;
                }
                catch (Exception ex)
                {
                    console.Error($"ERROR: reading {o.ConfigFilePath} - {ex.Message}");

                    // indicate we're done with errors in the configuration file
                    ReturnValue = ReturnValue.ConfigurationFileErrors;
                    return;
                }
            }

            // now see if there are parameters that override settings from the current settings.
            _appConfig.DocumentsRoot = Path.GetFullPath(o.DocFolder);
            _appConfig.ConfigFilePath = Path.GetFullPath(o.ConfigFilePath);

            if (o.Verbose)
            {
                _appConfig.Verbose = true;
            }

            if (o.Attachments)
            {
                _appConfig.DocLinkChecker.CheckForOrphanedResources = true;
            }

            if (o.Cleanup)
            {
                _appConfig.DocLinkChecker.CleanupOrphanedResources = true;
            }

            if (o.Table)
            {
                _appConfig.DocLinkChecker.ValidatePipeTableFormatting = true;
            }

            if (o.ValidateExternalLinks)
            {
                _appConfig.DocLinkChecker.ValidateExternalLinks = true;
            }
        }
    }
}
