namespace DocLinkChecker.Test
{
    using Bogus;
    using DocLinkChecker.Enums;
    using DocLinkChecker.Helpers;
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

    public class HyperlinkTests
    {
        private Faker _faker = new Faker();

        private Mock<HttpMessageHandler> _handler;
        private HttpClient _client;
        private AppConfig _config;
        private IFileService _fileService;
        private MockFileService _fileServiceMock;
        private ICustomConsoleLogger _console;
        private IServiceProvider _serviceProvider;

        public HyperlinkTests()
        {
            _config = new();
            _config.DocumentationFiles.SourceFolder = ".";
            _config.ResourceFolderNames = new() { ".attachmensts", "images" };

            _handler = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            _client = _handler.CreateClient();

            _fileServiceMock = new MockFileService();
            _fileServiceMock.FillDemoSet();
            _fileService = _fileServiceMock;

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

        [Fact]
        public async void ValidateExistingLocalLinkShouldNotHaveErrors()
        {
            // Arrange
            LinkValidatorService service = new LinkValidatorService(_serviceProvider, _config, _fileService, _console);

            //Act
            int line = 14;
            int column = 31;
            Hyperlink link = new Hyperlink($"{_fileServiceMock.Root}\\index.md", line, column, "getting-started/README.md");
            await service.VerifyHyperlink(link);

            // Assert
            service.Errors.Where(x => x.Severity == MarkdownErrorSeverity.Error).Should().BeEmpty();
        }

        [Fact]
        public async void ValidateNonExistingLocalLinkShouldHaveErrors()
        {
            // Arrange
            LinkValidatorService service = new LinkValidatorService(_serviceProvider, _config, _fileService, _console);

            //Act
            int line = 124;
            int column = 3381;
            Hyperlink link = new Hyperlink($"{_fileServiceMock.Root}\\start-document.md", line, column, "./non-existing-document.md");
            await service.VerifyHyperlink(link);

            // Assert
            service.Errors.Should().NotBeEmpty();
            service.Errors.First().Line.Should().Be(line);
            service.Errors.First().Column.Should().Be(column);
            service.Errors.First().Severity.Should().Be(Enums.MarkdownErrorSeverity.Error);
            service.Errors.First().Message.Should().Contain("Not found");
        }

        [Theory]
        [InlineData("../../another/docs/document-outside-docs-hierarchy.md", RelativeLinkType.SameDocsHierarchyOnly)]
        [InlineData("../../src/solution1/README.md", RelativeLinkType.SameDocsHierarchyOnly)]
        [InlineData("../../src/solution1/README.md", RelativeLinkType.AnyDocsHierarchy)]
        public async void ValidateLocalLinkOutsideDocsHierarchyShouldHaveErrors(string filename, RelativeLinkType strategy)
        {
            // Arrange
            _config.DocumentationFiles.SourceFolder = _fileServiceMock.Root;
            _config.DocLinkChecker.RelativeLinkStrategy = strategy;

            string path = Path.GetFullPath(Path.Combine(_fileServiceMock.Root, filename));
            _fileServiceMock.Files.Add(path, string.Empty
                .AddHeading("Document outside docs root", 1)
                .AddParagraphs(3));

            LinkValidatorService service = new LinkValidatorService(_serviceProvider, _config, _fileService, _console);

            //Act
            int line = 124;
            int column = 3381;
            Hyperlink link = new Hyperlink($"{_fileServiceMock.Root}\\index.md", line, column, filename);
            await service.VerifyHyperlink(link);

            // Assert
            service.Errors.Should().NotBeEmpty();
            service.Errors.First().Line.Should().Be(line);
            service.Errors.First().Column.Should().Be(column);
            service.Errors.First().Severity.Should().Be(MarkdownErrorSeverity.Error);
        }

        [Fact]
        public async void ValidateLocalLinkOutsideHierarchyWithConfigShouldNotHaveErrors()
        {
            // Arrange
            _config.DocumentationFiles.SourceFolder = _fileServiceMock.Root;
            _config.DocLinkChecker.RelativeLinkStrategy = RelativeLinkType.All;

            string filename = "../../src/solution1/README.md";
            string path = Path.GetFullPath(Path.Combine(_fileServiceMock.Root, filename));
            string empty = string.Empty;
            _fileServiceMock.Files.Add(path, empty
                .AddHeading("Document outside docs root", 1)
                .AddParagraphs(3));

            LinkValidatorService service = new LinkValidatorService(_serviceProvider, _config, _fileService, _console);

            //Act
            int line = 124;
            int column = 3381;
            Hyperlink link = new Hyperlink($"{_fileServiceMock.Root}\\index.md", line, column, filename);
            await service.VerifyHyperlink(link);

            // Assert
            service.Errors.Where(x => x.Severity == MarkdownErrorSeverity.Error).Should().BeEmpty();
        }

        [Theory]
        [InlineData("# [Linux](#tab/linux)")]
        [InlineData("# [Windows](#tab/windows)")]
        public async void ValidateTabHeadersShouldNotHaveErrors(string codeLink)
        {
            // Arrange
            _config.DocumentationFiles.SourceFolder = _fileServiceMock.Root;

            LinkValidatorService service = new LinkValidatorService(_serviceProvider, _config, _fileService, _console);

            //Act
            (List<MarkdownObjectBase> objects, List<MarkdownError> errors) result =
                MarkdownHelper.ParseMarkdownString($"{_fileServiceMock.Root}/start-document.md", codeLink, false);

            Heading heading = (Heading)result.objects.FirstOrDefault(result => result is Heading);
            Hyperlink link = (Hyperlink)result.objects.FirstOrDefault(result => result is Hyperlink);
            await service.VerifyHyperlink(link);

            // Assert
            heading.Should().NotBeNull();

            link.LinkType.Should().Be(HyperlinkType.Tab);

            service.Errors.Should().BeEmpty();
        }

        [Fact]
        public async void ValidateLocalLinkHeadingShouldNotHaveErrors()
        {
            // Arrange
            _config.DocumentationFiles.SourceFolder = _fileServiceMock.Root;
            string source = $"{_fileServiceMock.Root}\\general\\another-sample.md";

            LinkValidatorService service = new LinkValidatorService(_serviceProvider, _config, _fileService, _console);
            service.Headings.Add(new(source, 99, 1, "Third.1 Header", "third-1-header"));

            //Act
            int line = 432;
            int column = 771;

            Hyperlink link = new Hyperlink($"{_fileServiceMock.Root}\\index.md", line, column, $".\\general\\another-sample.md#third-1-header");
            await service.VerifyHyperlink(link);

            // Assert
            service.Errors.Should().BeEmpty();
        }

        [Theory]
        [InlineData("First header", "first-header", "#first-header")]
        [InlineData("`Second header`", "second-header", "#second-header")]
        public async void ValidateLocalLinkHeadingInSameDocumentShouldNotHaveErrors(string title, string id, string reference)
        {
            // Arrange
            _config.DocumentationFiles.SourceFolder = _fileServiceMock.Root;
            string source = $"{_fileServiceMock.Root}\\general\\another-sample.md";

            LinkValidatorService service = new LinkValidatorService(_serviceProvider, _config, _fileService, _console);
            service.Headings.Add(new(source, 99, 1, title, id));

            //Act
            int line = 432;
            int column = 771;

            Hyperlink link = new Hyperlink(source, line, column, reference);
            await service.VerifyHyperlink(link);

            // Assert
            service.Errors.Should().BeEmpty();
        }

        [Fact]
        public async void ValidateLocalLinkNonExistingHeadingShouldHaveErrors()
        {
            // Arrange
            _config.DocumentationFiles.SourceFolder = _fileServiceMock.Root;
            string sourceDoc = $"{_fileServiceMock.Root}\\general\\another-sample.md";
            string sourceHeadingTitle = "Some Heading in the Source document";
            string sourceHeadingId = "some-heading-in-the-source-document";
            string destDoc = $"{_fileServiceMock.Root}\\general\\general-sample.md";
            string destDocRelative = ".\\general-sample.md";
            int line = 432;
            int column = 771;

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
            _config.DocumentationFiles.SourceFolder = _fileServiceMock.Root;

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
            service.Errors.First().Severity.Should().Be(MarkdownErrorSeverity.Error);
            service.Errors.First().Message.Should().Contain("Full path not allowed");
        }

        [Theory]
        [InlineData("[!code-csharp[](src/sample.cs)]")]
        [InlineData("[!code-csharp[](src/sample.cs#region)]")]
        [InlineData("[!code-csharp[](src/sample.cs#L8-11)]")]
        [InlineData("[!code-csharp[](src/sample.cs?name=MainLoop)]")]
        [InlineData("[!code-csharp[](src/sample.cs?highlight=3,5)]")]
        [InlineData("[!INCLUDE [the defined way to include](getting-started/README.md)]")]
        [InlineData("[!include [using lowercase to include](getting-started/README.md)]")]
        public async void ValidateFileInclusionLinksShouldNotHaveErrors(string codeLink)
        {
            // Arrange
            _config.DocumentationFiles.SourceFolder = _fileServiceMock.Root;

            LinkValidatorService service = new LinkValidatorService(_serviceProvider, _config, _fileService, _console);

            //Act
            (List<MarkdownObjectBase> objects, List<MarkdownError> errors) result = 
                MarkdownHelper.ParseMarkdownString($"{_fileServiceMock.Root}/start-document.md", codeLink, false);

            Hyperlink link = (Hyperlink)result.objects.FirstOrDefault(result => result is Hyperlink);
            await service.VerifyHyperlink(link);

            // Assert
            service.Errors.Should().BeEmpty();
        }

        [Theory]
        [InlineData("[!code-csharp[](src/sample_NOT_EXISTING.cs)]")]
        [InlineData("[!code-csharp[](src/sample_NOT_EXISTING.cs#region)]")]
        [InlineData("[!code-csharp[](src/sample_NOT_EXISTING.cs#L8-11)]")]
        [InlineData("[!code-csharp[](src/sample_NOT_EXISTING.cs?name=MainLoop)]")]
        [InlineData("[!code-csharp[](src/sample_NOT_EXISTING.cs?highlight=3,5)]")]
        [InlineData("[!INCLUDE [the defined way to include](getting-started/README_NOT_EXISTING.md)]")]
        [InlineData("[!include [using lowercase to include](getting-started/README_NOT_EXISTING.md)]")]
        public async void ValidateFileInclusionLinksShouldHaveErrors(string codeLink)
        {
            // Arrange
            _config.DocumentationFiles.SourceFolder = _fileServiceMock.Root;

            LinkValidatorService service = new LinkValidatorService(_serviceProvider, _config, _fileService, _console);

            //Act
            (List<MarkdownObjectBase> objects, List<MarkdownError> errors) result =
                MarkdownHelper.ParseMarkdownString($"{_fileServiceMock.Root}/start-document.md", codeLink, false);

            Hyperlink link = (Hyperlink)result.objects.FirstOrDefault(result => result is Hyperlink);
            await service.VerifyHyperlink(link);

            // Assert
            service.Errors.Should().NotBeEmpty();
            service.Errors.First().Severity.Should().Be(MarkdownErrorSeverity.Error);
            service.Errors.First().Message.Should().Contain("Not found");
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
