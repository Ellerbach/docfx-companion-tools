// Licensed to DocFX Companion Tools and contributors under one or more agreements.
// DocFX Companion Tools and contributors licenses this file to you under the MIT license.

namespace DocLinkChecker
{
    using System;
    using System.Net.Http.Headers;
    using System.Text;
    using System.Threading.Tasks;
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
        /// <summary>
        /// Main entry point of the application.
        /// </summary>
        /// <param name="args">Commandline parameters.</param>
        /// <returns>Result of the process.</returns>
        public static Task<int> Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            CreateHostBuilder(args).Build().Run();
            return Task.FromResult(0);
        }

        /// <summary>
        /// Configure the application.
        /// </summary>
        /// <param name="args">Commandline parameters.</param>
        /// <returns><see cref="IHostBuilder"/>.</returns>
        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)

                // Ctrl-C
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
                    services.Configure<MdCheckerConfiguration>(hostContext.Configuration.GetSection("MdChecker"));

                    services.AddSingleton<Crawler>();
                    services.AddSingleton<Checker>();

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
                            MustRevalidate = true
                        };
                        c.Timeout = TimeSpan.FromSeconds(30);
                    })
                    .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
                    {
                        MaxConnectionsPerServer = 100,
                        // HttpClient does not support redirects from https to http:
                        // https://github.com/dotnet/runtime/issues/28039
                        AllowAutoRedirect = false,
                    })
                    .AddPolicyHandler(policy =>
                    {
                        return HttpPolicyExtensions
                            .HandleTransientHttpError()
                            .Or<TimeoutRejectedException>()
                            //.OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.NotFound)
                            //.OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                            .WaitAndRetryAsync(3, retry => TimeSpan.FromSeconds(1));
                    })
                    .AddTypedClient<CheckerHttpClient>();

                    services.AddHostedService<App>();
                });
        }
    }
}
