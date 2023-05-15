namespace DocLinkChecker.Test
{
    using Bogus;
    using DocLinkChecker.Enums;
    using DocLinkChecker.Interfaces;
    using DocLinkChecker.Models;
    using DocLinkChecker.Services;
    using FluentAssertions;
    using Microsoft.Extensions.DependencyInjection;
    using Moq;
    using Moq.Contrib.HttpClient;
    using System;
    using System.Linq;
    using System.Net;
    using System.Net.Http;

    public class HyperlinkTests
    {
        private Faker _faker = new Faker();

        private Mock<HttpMessageHandler> _handler;
        private HttpClient _client;
        private AppConfig _config;
        private IFileService _fileService;
        private ICustomConsoleLogger _console;
        private IServiceProvider _serviceProvider;

        public HyperlinkTests()
        {
            _config = new();
            _config.DocumentationFiles.SourceFolder = ".";
            _config.ResourceFolderNames = new() { ".attachmensts", "images" };

            _handler = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            _client = _handler.CreateClient();
            
            _fileService = new MockFileService();

            _console = GetMockedConsoleLogger();

            _serviceProvider = GetServiceProviderWithCheckerHttpClient(_client, _config);
        }

        [Fact]
        public async void ValidateExistingWebLinkShouldNotHaveError()
        {
            // Arrange
            // setup HttpClient moq
            _handler.SetupAnyRequest().ReturnsResponse(HttpStatusCode.OK);

            // setup file service

            LinkValidatorService service = new LinkValidatorService(_serviceProvider, _config, _fileService, _console);

            //Act
            Hyperlink link = new Hyperlink(_faker.System.FilePath(), 15, 8, "https://www.microsoft.com");
            await service.VerifyHyperlink(link);

            // Assert
            service.Errors.Should().BeEmpty();
        }

        [Fact]
        public async void ValidateNonExistingWebLinkShouldHaveErrors()
        {
            // Arrange
            // setup HttpClient moq
            _handler.SetupAnyRequest().ReturnsResponse(HttpStatusCode.NotFound);

            LinkValidatorService service = new LinkValidatorService(_serviceProvider, _config, _fileService, _console);

            //Act
            int line = 22;
            int column = 38;
            Hyperlink link = new Hyperlink(_faker.System.FilePath(), line, column, "https://www.microsoft.com/non-existing-path");
            await service.VerifyHyperlink(link);

            // Assert
            service.Errors.Should().NotBeEmpty();
            service.Errors.First().Line.Should().Be(line);
            service.Errors.First().Column.Should().Be(column);
            service.Errors.First().Severity.Should().Be(Enums.MarkdownErrorSeverity.Error);
            service.Errors.First().Message.Should().Contain("NotFound");
        }

        [Fact]
        public async void ValidateNonExistingDomainInWebLinkShouldHaveErrors()
        {
            // Arrange
            // setup HttpClient moq
            _handler.SetupAnyRequest().ReturnsResponse(HttpStatusCode.NotFound);

            LinkValidatorService service = new LinkValidatorService(_serviceProvider, _config, _fileService, _console);

            //Act
            int line = 22;
            int column = 38;
            Hyperlink link = new Hyperlink(_faker.System.FilePath(), line, column, "https://www.non-existing-domain-microsoft.com/non-existing-path");
            await service.VerifyHyperlink(link);

            // Assert
            service.Errors.Should().NotBeEmpty();
            service.Errors.First().Line.Should().Be(line);
            service.Errors.First().Column.Should().Be(column);
            service.Errors.First().Severity.Should().Be(Enums.MarkdownErrorSeverity.Error);
            service.Errors.First().Message.Should().Contain("No such host");
        }

        /* we only use the simplified version for now
        [Fact]
        public async void ValidateNormalRedirectLinkShouldNotHaveErrors()
        {
            // Arrange
            string requestUrl = "https://www.microsoft.com/redirect-path";
            string redirectedUrl1 = "https://www.microsoft.com/redirected-path";
            string redirectedUrl2 = "https://www.microsoft.com/end-path";

            // setup HttpClient moq
            _handler.SetupRequest(HttpMethod.Get, requestUrl).ReturnsResponse(HttpStatusCode.Redirect, configure: response =>
            {
                response.Headers.Location = new Uri(redirectedUrl1);
            });
            _handler.SetupRequest(HttpMethod.Get, redirectedUrl1).ReturnsResponse(HttpStatusCode.Redirect, configure: response =>
            {
                response.Headers.Location = new Uri(redirectedUrl2);
            });
            _handler.SetupRequest(HttpMethod.Get, redirectedUrl2).ReturnsResponse(HttpStatusCode.OK);

            LinkValidatorService service = new LinkValidatorService(_serviceProvider, _config, _fileService, _console);

            //Act
            int line = 22;
            int column = 38;
            Hyperlink link = new Hyperlink(_faker.System.FilePath(), line, column, requestUrl);
            await service.VerifyHyperlink(link);

            // Assert
            service.Errors.Should().BeEmpty();
        }
        */

        /* we only use the simplified version for now
        [Fact]
        public async void ValidateWebLinkWithTooManyRedirectsShouldHaveErrors()
        {
            // Arrange
            _config.DocLinkChecker.MaxHttpRedirects = 2;

            string requestUrl = "https://www.microsoft.com/redirect-path";
            string redirectedUrl1 = "https://www.microsoft.com/redirected-path";
            string redirectedUrl2 = "https://www.microsoft.com/end-path";

            // setup HttpClient moq
            _handler.SetupRequest(HttpMethod.Get, requestUrl).ReturnsResponse(HttpStatusCode.Redirect, configure: response =>
            {
                response.Headers.Location = new Uri(redirectedUrl1);
            });
            _handler.SetupRequest(HttpMethod.Get, redirectedUrl1).ReturnsResponse(HttpStatusCode.Redirect, configure: response =>
            {
                response.Headers.Location = new Uri(redirectedUrl2);
            });
            _handler.SetupRequest(HttpMethod.Get, redirectedUrl2).ReturnsResponse(HttpStatusCode.OK);

            LinkValidatorService service = new LinkValidatorService(_serviceProvider, _config, _fileService, _console);

            //Act
            int line = 22;
            int column = 38;
            Hyperlink link = new Hyperlink(_faker.System.FilePath(), line, column, requestUrl);
            await service.VerifyHyperlink(link);

            // Assert
            service.Errors.Should().NotBeEmpty();
            service.Errors.First().Line.Should().Be(line);
            service.Errors.First().Column.Should().Be(column);
            service.Errors.First().Severity.Should().Be(Enums.MarkdownErrorSeverity.Error);
            service.Errors.First().Message.Should().Contain("Excessive number of redirects");
        }
        */

        [Fact]
        public async void ValidateExistingLocalLinkShouldNotHaveErrors()
        {
            // Arrange
            ((MockFileService)_fileService).Exists = true;

            LinkValidatorService service = new LinkValidatorService(_serviceProvider, _config, _fileService, _console);

            //Act
            int line = 14;
            int column = 31;
            Hyperlink link = new Hyperlink("./start-document.md", line, column, "./an-existing-document.md");
            await service.VerifyHyperlink(link);

            // Assert
            service.Errors.Where(x => x.Severity == MarkdownErrorSeverity.Error).Should().BeEmpty();
        }

        [Fact]
        public async void ValidateNonExistingLocalLinkShouldHaveErrors()
        {
            // Arrange
            ((MockFileService)_fileService).Exists = false;

            LinkValidatorService service = new LinkValidatorService(_serviceProvider, _config, _fileService, _console);

            //Act
            int line = 124;
            int column = 3381;
            Hyperlink link = new Hyperlink("./start-document.md", line, column, "./non-existing-document.md");
            await service.VerifyHyperlink(link);

            // Assert
            service.Errors.Should().NotBeEmpty();
            service.Errors.First().Line.Should().Be(line);
            service.Errors.First().Column.Should().Be(column);
            service.Errors.First().Severity.Should().Be(Enums.MarkdownErrorSeverity.Error);
            service.Errors.First().Message.Should().Contain("Not found");
        }

        [Fact]
        public async void ValidateLocalLinkOutsideDocsHierarchyShouldHaveErrors()
        {
            // Arrange
            _config.DocumentationFiles.SourceFolder = @"d:\Git\Project\docs";
            _config.DocLinkChecker.AllowLinksOutsideDocumentsRoot = false;
            ((MockFileService)_fileService).Exists = true;

            LinkValidatorService service = new LinkValidatorService(_serviceProvider, _config, _fileService, _console);

            //Act
            int line = 124;
            int column = 3381;
            Hyperlink link = new Hyperlink(@"d:\Git\Projects\docs\start-document.md", line, column, @"..\document-outside-docs-hierarchy.md");
            await service.VerifyHyperlink(link);

            // Assert
            service.Errors.Should().NotBeEmpty();
            service.Errors.First().Line.Should().Be(line);
            service.Errors.First().Column.Should().Be(column);
            service.Errors.First().Severity.Should().Be(Enums.MarkdownErrorSeverity.Error);
            service.Errors.First().Message.Should().Contain("referenced outside of the docs hierarchy not allowed");
        }

        [Fact]
        public async void ValidateLocalLinkOutsideHierarchyWithConfigShouldNotHaveErrors()
        {
            // Arrange
            _config.DocumentationFiles.SourceFolder = @"d:\Git\Project\docs";
            _config.DocLinkChecker.AllowLinksOutsideDocumentsRoot = true;
            ((MockFileService)_fileService).Exists = true;

            LinkValidatorService service = new LinkValidatorService(_serviceProvider, _config, _fileService, _console);

            //Act
            int line = 124;
            int column = 3381;
            Hyperlink link = new Hyperlink(@"d:\Git\Projects\docs\start-document.md", line, column, @"..\document-outside-docs-hierarchy.md");
            await service.VerifyHyperlink(link);

            // Assert
            service.Errors.Where(x => x.Severity == MarkdownErrorSeverity.Error).Should().BeEmpty();
            service.Errors.Count(x => x.Severity == MarkdownErrorSeverity.Warning).Should().Be(1);
        }

        [Fact]
        public async void ValidateLocalLinkHeadingShouldNotHaveErrors()
        {
            // Arrange
            string sourceDoc = @"d:\git\project\docs\source.md";
            string sourceHeadingTitle = "Some Heading in the Source document";
            string sourceHeadingId = "some-heading-in-the-source-document";
            string destDoc = @"d:\git\project\docs\dest.md";
            string destDocRelative = @".\dest.md";
            int line = 432;
            int column = 771;

            ((MockFileService)_fileService).Exists = true;
            _config.DocumentationFiles.SourceFolder = @"d:\git\project\docs";

            LinkValidatorService service = new LinkValidatorService(_serviceProvider, _config, _fileService, _console);
            service.Headings.Add(new(destDoc, 99, 1, sourceHeadingTitle, sourceHeadingId));

            //Act
            Hyperlink link = new Hyperlink(sourceDoc, line, column, $"{destDocRelative}#{sourceHeadingId}");
            await service.VerifyHyperlink(link);

            // Assert
            service.Errors.Should().BeEmpty();
        }

        [Fact]
        public async void ValidateLocalLinkHeadingInSameDocumentShouldNotHaveErrors()
        {
            // Arrange
            string sourceDoc = @"d:\git\project\docs\source.md";
            string sourceHeadingTitle = "Some Heading in the Source document";
            string sourceHeadingId = "some-heading-in-the-source-document";
            int line = 432;
            int column = 771;

            ((MockFileService)_fileService).Exists = true;
            _config.DocumentationFiles.SourceFolder = @"d:\git\project\docs";

            LinkValidatorService service = new LinkValidatorService(_serviceProvider, _config, _fileService, _console);
            service.Headings.Add(new(sourceDoc, 99, 1, sourceHeadingTitle, sourceHeadingId));

            //Act
            Hyperlink link = new Hyperlink(sourceDoc, line, column, $"#{sourceHeadingId}");
            await service.VerifyHyperlink(link);

            // Assert
            service.Errors.Should().BeEmpty();
        }

        [Fact]
        public async void ValidateLocalLinkNonExistingHeadingShouldHaveErrors()
        {
            // Arrange
            string sourceDoc = @"d:\git\project\docs\source.md";
            string sourceHeadingTitle = "Some Heading in the Source document";
            string sourceHeadingId = "some-heading-in-the-source-document";
            string destDoc = @"d:\git\project\docs\dest.md";
            string destDocRelative = @".\dest.md";
            int line = 432;
            int column = 771;

            ((MockFileService)_fileService).Exists = true;
            _config.DocumentationFiles.SourceFolder = @"d:\git\project\docs";

            LinkValidatorService service = new LinkValidatorService(_serviceProvider, _config, _fileService, _console);
            service.Headings.Add(new(destDoc, 99, 1, sourceHeadingTitle, sourceHeadingId));

            //Act
            Hyperlink link = new Hyperlink(sourceDoc, line, column, $"{destDocRelative}#non-existing-header");
            await service.VerifyHyperlink(link);

            // Assert
            service.Errors.Should().NotBeEmpty();
            var headingError = service.Errors.FirstOrDefault(x => x.Message.Contains("Heading"));
            headingError.Line.Should().Be(line);
            headingError.Column.Should().Be(column);
            headingError.Severity.Should().Be(Enums.MarkdownErrorSeverity.Warning);
        }

        [Fact]
        public async void ValidateLocalLinkWithFullPathShouldHaveErrors()
        {
            // Arrange
            ((MockFileService)_fileService).Exists = true;

            LinkValidatorService service = new LinkValidatorService(_serviceProvider, _config, _fileService, _console);

            //Act
            int line = 124;
            int column = 3381;
            Hyperlink link = new Hyperlink("./start-document.md", line, column, @"D:\Git\Project\docs\another-document.md");
            await service.VerifyHyperlink(link);

            // Assert
            service.Errors.Should().NotBeEmpty();
            service.Errors.First().Line.Should().Be(line);
            service.Errors.First().Column.Should().Be(column);
            service.Errors.First().Severity.Should().Be(Enums.MarkdownErrorSeverity.Error);
            service.Errors.First().Message.Should().Contain("Full path not allowed");
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
