// <copyright file="LiquidServiceTests.cs" company="DocFx Companion Tools">
// Copyright (c) DocFx Companion Tools. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
using Bogus;
using DocFxTocGenerator.Actions;
using DocFxTocGenerator.ConfigFiles;
using DocFxTocGenerator.FileService;
using DocFxTocGenerator.Liquid;
using DocFxTocGenerator.Test.Helpers;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace DocFxTocGenerator.Test;

public class LiquidServiceTests
{
    private Faker _faker = new();
    private MockFileService _fileService = new();
    private MockLogger _mockLogger = new();
    private ILogger _logger;
    private ConfigFilesService _config;
    private readonly ITestOutputHelper _outputHelper;

    public LiquidServiceTests(ITestOutputHelper outputHelper)
    {
        _outputHelper = outputHelper;
        _fileService.FillDemoSet();
        _logger = _mockLogger.Logger;
        _config = new(_fileService, _logger);
    }

    [Fact]
    public async Task Render_EmptyTemplate_ReturnsEmpty()
    {
        _outputHelper.WriteLine($"XXTEST: HIERO");
        _outputHelper.WriteLine($"XXTEST: IsRooted1 {System.IO.Path.IsPathRooted("C:\\")}");
        _outputHelper.WriteLine($"XXTEST: IsRooted2 {System.IO.Path.IsPathRooted("/mnt/c")}");

        string fullRoot = _fileService.GetFullPath(_fileService.Root);
        _outputHelper.WriteLine($"XXTEST: FullRoot = {fullRoot}");
        var query = _fileService.Files.AsQueryable();
        // fixed query for includes. Fine for this testing.
        query = query.Where(x => x.Value != string.Empty &&
                                 x.Key.StartsWith(fullRoot) &&
                                 (x.Key.EndsWith(".md") || x.Key.EndsWith("swagger.json")));
        var list = query.Select(x => x.Key.NormalizePath()).ToList();
        _outputHelper.WriteLine($"XXTEST: Found {list.Count} entries.");
        _outputHelper.WriteLine($"XXTEST: First: {list.First()}");

        // arrange
        LiquidService service = new(_logger);
        ContentInventoryAction action = new(_fileService.Root, useOrder: true, useIgnore: true, useOverride: true, camelCasing: false, _fileService, _logger);

        // act
        await action.RunAsync();
        FolderData? current = action!.RootFolder!.Find("continents/americas/brasil");

        // act
        string content = service.Render(action.RootFolder, current!, string.Empty);

        // assert
        content.Should().BeEmpty();
    }

    [Fact]
    public async Task Render_SimpleTemplate_ReturnsValid()
    {
        // arrange
        LiquidService service = new(_logger);
        ContentInventoryAction action = new(_fileService.Root, useOrder: true, useIgnore: true, useOverride: true, camelCasing: false, _fileService, _logger);

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
    public async Task Render_ErrorTemplate_ThrowsException()
    {
        // arrange
        LiquidService service = new(_logger);
        ContentInventoryAction action = new(_fileService.Root, useOrder: true, useIgnore: true, useOverride: true, camelCasing: false, _fileService, _logger);

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
    public async Task Render_ErrorTemplateObjectReference_ThrowsException()
    {
        // arrange
        LiquidService service = new(_logger);
        ContentInventoryAction action = new(_fileService.Root, useOrder: true, useIgnore: true, useOverride: true, camelCasing: false, _fileService, _logger);

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
