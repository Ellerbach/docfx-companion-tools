// Licensed to DocFX Companion Tools and contributors under one or more agreements.
// DocFX Companion Tools and contributors licenses this file to you under the MIT license.

namespace DocLinkChecker
{
    using System;
    using System.IO;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Text;
    using System.Text.Json;
    using System.Text.Json.Serialization;
    using System.Threading.Tasks;
    using CommandLine;
    using DocLinkChecker.Helpers;
    using DocLinkChecker.Models;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Polly;
    using Polly.Extensions.Http;
    using Polly.Timeout;

    /// <summary>
    /// Main program class for documentation link checker tool. It's a command-line tool
    /// that takes parameters. Use -help as parameter to see the syntax.
    /// </summary>
    public class Program
    {
        private const string AppConfigName = "docfx-companion-tools.json";
        private static AppConfig _appConfig = new ();

        /// <summary>
        /// Main entry point of the application.
        /// </summary>
        /// <param name="args">Commandline parameters.</param>
        /// <returns>Result of the process.</returns>
        public static Task<int> Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;

            if (File.Exists(AppConfigName))
            {
                // we have a local configuration file, so read it.
                string json = File.ReadAllText(AppConfigName);
                _appConfig = JsonSerializer.Deserialize<AppConfig>(json);
            }

            // parse flags that can overwrite settings from configuration.
            Parser.Default.ParseArguments<CommandlineOptions>(args)
                                   .WithParsed<CommandlineOptions>(ProcessSettings);

            CreateHostBuilder(args).Build().Run();
            return Task.FromResult(0);
        }

        /// <summary>
        /// Run the logic of the app with the given parameters.
        /// Given folders are checked if they exist.
        /// </summary>
        /// <param name="o">Parsed commandline options.</param>
        private static void ProcessSettings(CommandlineOptions o)
        {
        }

        /// <summary>
        /// Configure the application.
        /// </summary>
        /// <param name="args">Commandline parameters.</param>
        /// <returns><see cref="IHostBuilder"/>.</returns>
        public static IHostBuilder CreateHostBuilder(string[] args)
        {
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
                    ////services.Configure<AppConfig>(hostContext.Configuration.GetSection("MdChecker"));

                    ////services.AddSingleton<Crawler>();
                    ////services.AddSingleton<Checker>();

                    services.AddHttpClient("MdChecker-Client", c =>
                    {
                        c.DefaultRequestHeaders.Add("User-Agent", "MdChecker Http Client");
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
    }
}
