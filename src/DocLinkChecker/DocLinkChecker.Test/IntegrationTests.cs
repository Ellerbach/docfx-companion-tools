namespace DocLinkChecker.Test
{
    using Bogus;
    using DocLinkChecker.Enums;
    using DocLinkChecker.Interfaces;
    using DocLinkChecker.Models;
    using DocLinkChecker.Services;
    using DocLinkChecker.Test.Helpers;
    using FluentAssertions;
    using Microsoft.Extensions.DependencyInjection;
    using Moq;
    using Moq.Contrib.HttpClient;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;

    public class IntegrationTests
    {
        private Faker _faker = new Faker();

        private Mock<HttpMessageHandler> _handler;
        private HttpClient _client;
        private AppConfig _config;
        private IFileService _fileService;
        private MockFileService _fileServiceMock;
        private ICustomConsoleLogger _console;
        private IServiceProvider _serviceProvider;

        public IntegrationTests()
        {
            _fileServiceMock = new MockFileService();
            _fileServiceMock.FillDemoSet();
            _fileService = _fileServiceMock;

            _config = new();
            _config.DocumentationFiles.SourceFolder = ".";
            _config.ResourceFolderNames = new() { ".attachmensts", "images" };

            _handler = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            _client = _handler.CreateClient();

            _console = GetMockedConsoleLogger();

            _serviceProvider = GetServiceProviderWithCheckerHttpClient(_client, _config);
        }

        [Fact]
        public async void ValidateDemoFilesShouldHaveNoErrors()
        {
            List<MarkdownError> errors = new();

            // get all information from markdown files
            CrawlerService crawler = new CrawlerService(_config, _console, _fileService, logger ?);
            var parsed = await _crawler.ParseMarkdownFiles();
            errors.AddRange(parsed.errors);

            // add the headings to the _checker to validate heading references
            var headings = parsed.objects.OfType<Heading>().ToList();
            _linkValidator.Headings.AddRange(headings);

            // validate the links
            List<Hyperlink> links = parsed.objects.OfType<Hyperlink>().ToList();
            foreach (Hyperlink link in links)
            {
                if (!link.IsWeb || _config.DocLinkChecker.ValidateExternalLinks)
                {
                    _linkValidator.EnqueueLinkForValidation(link);
                }
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
                if (resourceValidationResult.orphanedResources.Any() &&
                    _config.DocLinkChecker.CleanupOrphanedResources &&
                    !errors.Any())
                {
                    // if there are orphaned resources AND
                    // AND we didn't have any other errors ... cleanup
                    // This is done, as errors can indicate mistake(s) in the links. That has to be
                    // corrected first, before we can cleanup the orphaned resources.
                    _resourceValidator.CleanupOrphanedResources(resourceValidationResult.orphanedResources);
                }

                errors.AddRange(resourceValidationResult.errors);
            }

            // sort the errors on file and file position and show them
            errors = errors.OrderBy(x => x.MarkdownFilePath).ThenBy(x => x.Line).ThenBy(x => x.Column).ToList();
            if (errors.Any())
            {
                if (errors.FirstOrDefault(x => x.Severity == MarkdownErrorSeverity.Error) != null)
                {
                    Program.ReturnValue = ReturnValue.Errors;
                }
                else if (errors.FirstOrDefault(x => x.Severity == MarkdownErrorSeverity.Warning) != null)
                {
                    Program.ReturnValue = ReturnValue.WarningsOnly;
                }
                else
                {
                    Program.ReturnValue = ReturnValue.Success;
                }
            }
        }

        private ICustomConsoleLogger GetMockedConsoleLogger()
        {
            Mock<ICustomConsoleLogger> console = new Mock<ICustomConsoleLogger>();
            console.Setup(x => x.Output(It.IsAny<string>()));
            console.Setup(x => x.Verbose(It.IsAny<string>()));
            console.Setup(x => x.Warning(It.IsAny<string>()));
            console.Setup(x => x.Error(It.IsAny<string>()));
            return console.Object;
        }

        private IServiceProvider GetServiceProviderWithCheckerHttpClient(HttpClient client, AppConfig config)
        {
            CheckerHttpClient checkerService = new CheckerHttpClient(client, config);
            ServiceCollection serviceCollection = new ServiceCollection();
            serviceCollection.AddScoped(x => checkerService);
            return serviceCollection.BuildServiceProvider();
        }
    }
}