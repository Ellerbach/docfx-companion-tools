using Bogus;
using DocFxTocGenerator.Actions;
using DocFxTocGenerator.ConfigFiles;
using DocFxTocGenerator.FileService;
using DocFxTocGenerator.Liquid;
using DocFxTocGenerator.Test.Helpers;
using FluentAssertions;
using Microsoft.Extensions.Logging;

namespace DocFxTocGenerator.Test;

public class LiquidServiceTests
{
    private Faker _faker = new();
    private MockFileService _fileService = new();
    private ILogger _logger;
    private ConfigFilesService _config;

    public LiquidServiceTests()
    {
        _fileService.FillDemoSet();
        _logger = MockLogger.GetMockedLogger();
        _config = new(_fileService, _logger);
    }

    [Fact]
    public async void Render_EmptyTemplate_ReturnsEmpty()
    {
        // arrange
        LiquidService service = new(_logger);
        ContentInventoryAction action = new(_fileService.Root, _fileService.Root, useOrder: true, useIgnore: true, useOverride: true, _fileService, _logger);

        // act
        await action.RunAsync();
        FolderData? current = action!.RootFolder!.Find("continents/americas/brasil");

        // act
        string content = service.Render(action.RootFolder, current!, string.Empty);

        // assert
        content.Should().BeEmpty();
    }

    [Fact]
    public async void Render_SimpleTemplate_ReturnsValid()
    {
        // arrange
        LiquidService service = new(_logger);
        ContentInventoryAction action = new(_fileService.Root, _fileService.Root, useOrder: true, useIgnore: true, useOverride: true, _fileService, _logger);

        // act
        await action.RunAsync();
        FolderData? current = action!.RootFolder!.Find("continents/americas/brasil");

        string template = "Root: {{ root.Path }}\nCurrent: {{ current.Path }}";
        string expected = $"Root: {action.RootFolder.Path}\nCurrent: {current!.Path}";

        // act
        string content = service.Render(action.RootFolder, current!, template);

        // assert
        content.Should().Be(expected);
    }

    [Fact]
    public async void Render_ErrorTemplate_ThrowsException()
    {
        // arrange
        LiquidService service = new(_logger);
        ContentInventoryAction action = new(_fileService.Root, _fileService.Root, useOrder: true, useIgnore: true, useOverride: true, _fileService, _logger);

        // act
        await action.RunAsync();
        FolderData? current = action!.RootFolder!.Find("continents/americas/brasil");

        // error template: missing closing } after root.Path
        string template = "Root: {{ root.Path }\nCurrent: {{ current.Path }}";

        // act & assert
        var exception = Assert.Throws<LiquidException>(() => service.Render(action.RootFolder, current!, template));
        exception.Message.Should().StartWith("Parse error:");
    }

    [Fact]
    public async void Render_ErrorTemplateObjectReference_ThrowsException()
    {
        // arrange
        LiquidService service = new(_logger);
        ContentInventoryAction action = new(_fileService.Root, _fileService.Root, useOrder: true, useIgnore: true, useOverride: true, _fileService, _logger);

        // act
        await action.RunAsync();
        FolderData? current = action!.RootFolder!.Find("continents/americas/brasil");

        // error template: referencing nonexisting object
        string template = "{% include \"non-existing\" %}";

        // act & assert
        var exception = Assert.Throws<LiquidException>(() => service.Render(action.RootFolder, current!, template));
        exception.Message.Should().StartWith("non-existing.liquid");
    }
}
