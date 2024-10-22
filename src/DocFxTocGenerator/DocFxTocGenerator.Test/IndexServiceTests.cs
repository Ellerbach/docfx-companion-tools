using Bogus;
using DocFxTocGenerator.Actions;
using DocFxTocGenerator.ConfigFiles;
using DocFxTocGenerator.FileService;
using DocFxTocGenerator.Index;
using DocFxTocGenerator.TableOfContents;
using DocFxTocGenerator.Test.Helpers;
using FluentAssertions;
using Microsoft.Extensions.Logging;

namespace DocFxTocGenerator.Test;

public class IndexServiceTests
{
    private Faker _faker = new();
    private MockFileService _fileService = new();
    private ILogger _logger;
    private ConfigFilesService _config;

    public IndexServiceTests()
    {
        _fileService.FillDemoSet();
        _logger = MockLogger.GetMockedLogger();
        _config = new(_fileService, _logger);
    }

    [Fact]
    public async void GenerateIndex_WithoutIndexTemplateFile()
    {
        // arrange
        IndexService service = new(_fileService, _logger);
        ContentInventoryAction action = new(_fileService.Root, useOrder: true, useIgnore: true, useOverride: true, _fileService, _logger);

        // act
        await action.RunAsync();
        FolderData? current = action!.RootFolder!.Find("continents/americas/brasil");

        // act
        var path = _fileService.GetFullPath(service.GenerateIndex(action.RootFolder, current!));

        // assert
        path.Should().NotBeNullOrEmpty();

        string content = _fileService.ReadAllText(path);
        // validating the default template rendering as well here
        content.Replace("\r", "").Should().Be("# Brasil\n\n* [Sao Paulo](sao-paulo.md)\n* [Nova Friburgo](nova-friburgo.md)\n* [Rio de Janeiro](rio-de-janeiro.md)\n\n");

        // cleanup index file
        _fileService.Delete(path);
    }

    [Fact]
    public async void GenerateIndex_WithIndexTemplateFileInSameFolde()
    {
        // arrange
        IndexService service = new(_fileService, _logger);
        ContentInventoryAction action = new(_fileService.Root,  useOrder: true, useIgnore: true, useOverride: true, _fileService, _logger);

        // act
        await action.RunAsync();
        FolderData? current = action!.RootFolder!.Find("continents/americas/brasil");

        string template = "This is a custom index template.";
        _fileService.AddFile(current!.Path, ".index.liquid", template);

        // act
        var path = _fileService.GetFullPath(service.GenerateIndex(action.RootFolder, current!));

        // assert
        path.Should().NotBeNullOrEmpty();

        string content = _fileService.ReadAllText(path);
        content.Should().Be(template);

        // cleanup index file
        _fileService.Delete(path);
    }

    [Fact]
    public async void GenerateIndex_WithIndexTemplateFileInDocRootFolder()
    {
        // arrange
        IndexService service = new(_fileService, _logger);
        ContentInventoryAction action = new(_fileService.Root, useOrder: true, useIgnore: true, useOverride: true, _fileService, _logger);

        // act
        await action.RunAsync();
        FolderData? current = action!.RootFolder!.Find("continents/americas/brasil");

        string template = "This is a custom index template from document root.";
        _fileService.AddFile(action.RootFolder.Path, ".index.liquid", template);

        // act
        var path = _fileService.GetFullPath(service.GenerateIndex(action.RootFolder, current!));

        // assert
        path.Should().NotBeNullOrEmpty();

        string content = _fileService.ReadAllText(path);
        content.Should().Be(template);

        // cleanup index file
        _fileService.Delete(path);
    }

    [Fact]
    public async void GenerateIndex_WithIndexTemplateFileInDiscRootFolder()
    {
        // arrange
        IndexService service = new(_fileService, _logger);
        ContentInventoryAction action = new(_fileService.Root, useOrder: true, useIgnore: true, useOverride: true, _fileService, _logger);

        // act
        await action.RunAsync();
        FolderData? current = action!.RootFolder!.Find("continents/americas/brasil");

        string template = "This is a custom index template from disk root.";
        // d:\\Git\\Project\\docs
        _fileService.AddFolder("d:/");
        _fileService.AddFolder("d:/git");
        _fileService.AddFolder("d:/git/projects");
        _fileService.AddFile("d:/", ".index.liquid", template);

        // act
        var path = _fileService.GetFullPath(service.GenerateIndex(action.RootFolder, current!));

        // assert
        path.Should().NotBeNullOrEmpty();

        string content = _fileService.ReadAllText(path);
        content.Should().Be(template);

        // cleanup index file
        _fileService.Delete(path);
    }
}
