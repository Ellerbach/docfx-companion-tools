using Bogus;
using DocFxTocGenerator.FileService;
using DocFxTocGenerator.Index;
using DocFxTocGenerator.Test.Helpers;
using FluentAssertions;
using Microsoft.Extensions.Logging;

namespace DocFxTocGenerator.Test;

public class IndexServiceTests
{
    private Faker _faker = new();
    private MockFileService _fileService = new();
    private ILogger _logger;

    public IndexServiceTests()
    {
        _fileService.FillDemoSet();
        _logger = MockLogger.GetMockedLogger();
    }

    [Fact]
    public void GenerateIndex_WithoutIndexTemplateFile()
    {
        // arrange
        IndexService service = new(_fileService, _logger);
        FolderData? root = _fileService.GetFolderDataStructure();
        FolderData? current = _fileService.FindFolderData(root!, "continents/americas/brasil");

        // act
        var path = _fileService.GetFullPath(service.GenerateIndex(root!, current!));

        // assert
        path.Should().NotBeNullOrEmpty();

        string content = _fileService.ReadAllText(path);
        // validating the default template rendering as well here
        content.Replace("\r", "").Should().Be("# brasil\n\n* [nova-friburgo](nova-friburgo.md)\n* [rio-de-janeirio](rio-de-janeirio.md)\n* [sao-paulo](sao-paulo.md)\n\n");

        // cleanup index file
        _fileService.Delete(path);
    }

    [Fact]
    public void GenerateIndex_WithIndexTemplateFileInSameFolder()
    {
        // arrange
        IndexService service = new(_fileService, _logger);
        FolderData? root = _fileService.GetFolderDataStructure();
        FolderData? current = _fileService.FindFolderData(root!, "continents/americas/brasil");

        string template = "This is a custom index template.";
        _fileService.AddFile(current!.Path, ".index.liquid", template);

        // act
        var path = _fileService.GetFullPath(service.GenerateIndex(root!, current!));

        // assert
        path.Should().NotBeNullOrEmpty();

        string content = _fileService.ReadAllText(path);
        content.Should().Be(template);

        // cleanup index file
        _fileService.Delete(path);
    }

    [Fact]
    public void GenerateIndex_WithIndexTemplateFileInDocRootFolder()
    {
        // arrange
        IndexService service = new(_fileService, _logger);
        FolderData? root = _fileService.GetFolderDataStructure();
        FolderData? current = _fileService.FindFolderData(root!, "continents/americas/brasil");

        string template = "This is a custom index template from document root.";
        _fileService.AddFile(root!.Path, ".index.liquid", template);

        // act
        var path = _fileService.GetFullPath(service.GenerateIndex(root!, current!));

        // assert
        path.Should().NotBeNullOrEmpty();

        string content = _fileService.ReadAllText(path);
        content.Should().Be(template);

        // cleanup index file
        _fileService.Delete(path);
    }

    [Fact]
    public void GenerateIndex_WithIndexTemplateFileInDiscRootFolder()
    {
        // arrange
        IndexService service = new(_fileService, _logger);
        FolderData? root = _fileService.GetFolderDataStructure();
        FolderData? current = _fileService.FindFolderData(root!, "continents/americas/brasil");

        string template = "This is a custom index template from disk root.";
        // d:\\Git\\Project\\docs
        _fileService.AddFolder("d:/");
        _fileService.AddFolder("d:/git");
        _fileService.AddFolder("d:/git/projects");
        _fileService.AddFile("d:/", ".index.liquid", template);

        // act
        var path = _fileService.GetFullPath(service.GenerateIndex(root!, current!));

        // assert
        path.Should().NotBeNullOrEmpty();

        string content = _fileService.ReadAllText(path);
        content.Should().Be(template);

        // cleanup index file
        _fileService.Delete(path);
    }
}
