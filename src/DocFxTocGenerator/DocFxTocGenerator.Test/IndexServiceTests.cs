// <copyright file="IndexServiceTests.cs" company="DocFx Companion Tools">
// Copyright (c) DocFx Companion Tools. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
using Bogus;
using DocFxTocGenerator.Actions;
using DocFxTocGenerator.ConfigFiles;
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
    private MockLogger _mockLogger = new();
    private ILogger _logger;
    private ConfigFilesService _config;

    public IndexServiceTests()
    {
        _fileService.FillDemoSet();
        _logger = _mockLogger.Logger;
        _config = new(camelCasing: false, _fileService, _logger);
    }

    [Fact]
    public async Task GenerateIndex_WithoutIndexTemplateFile()
    {
        // arrange
        IndexService service = new(_fileService, _logger);
        ContentInventoryAction action = new(_fileService.Root, useOrder: true, useIgnore: true, useOverride: true, camelCasing: false, _fileService, _logger);

        // act
        await action.RunAsync();
        FolderData? current = action!.RootFolder!.Find("continents/americas/brasil");

        // act
        var path = _fileService.GetFullPath(service.GenerateIndex(action.RootFolder, current!));

        // assert
        path.Should().NotBeNullOrEmpty();

        string content = _fileService.ReadAllText(path).Replace("\r", "");
        // validating the default template rendering as well here
        content.Should().Be("# Brasil\n\n* [Sao Paulo](sao-paulo.md)\n* [Nova Friburgo](nova-friburgo.md)\n* [Rio de Janeiro](rio-de-janeiro.md)\n\n");

        // cleanup index file
        _fileService.Delete(path);
    }

    [Fact]
    public async Task GenerateIndex_WithIndexTemplateFileInSameFolde()
    {
        // arrange
        IndexService service = new(_fileService, _logger);
        ContentInventoryAction action = new(_fileService.Root,  useOrder: true, useIgnore: true, useOverride: true, camelCasing: false, _fileService, _logger);

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
    public async Task GenerateIndex_WithIndexTemplateFileInDocRootFolder()
    {
        // arrange
        IndexService service = new(_fileService, _logger);
        ContentInventoryAction action = new(_fileService.Root, useOrder: true, useIgnore: true, useOverride: true, camelCasing: false, _fileService, _logger);

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
    public async Task GenerateIndex_WithIndexTemplateFileInDiscRootFolder()
    {
        // arrange
        IndexService service = new(_fileService, _logger);
        ContentInventoryAction action = new(_fileService.Root, useOrder: true, useIgnore: true, useOverride: true, camelCasing: false, _fileService, _logger);

        // act
        await action.RunAsync();
        FolderData? current = action!.RootFolder!.Find("continents/americas/brasil");

        string template = "This is a custom index template from disk root.";

        string[] subPaths = _fileService.Root.Split('/');
        string rootPath = subPaths[0];
        string p = string.Empty;
        foreach (string subPath in subPaths)
        {
            if (!string.IsNullOrEmpty(subPath))
            {
                p = Path.Combine(p, subPath);
                if (!_fileService.ExistsFileOrDirectory(p))
                {
                    _fileService.AddFolder(p);
                }
            }
        }
        _fileService.AddFile(rootPath, ".index.liquid", template);

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
