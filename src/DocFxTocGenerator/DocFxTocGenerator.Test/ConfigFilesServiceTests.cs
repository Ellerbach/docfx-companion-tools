// <copyright file="ConfigFileServiceTests.cs" company="DocFx Companion Tools">
// Copyright (c) DocFx Companion Tools. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
using System.Text;
using System.Text.RegularExpressions;
using Bogus;
using DocFxTocGenerator.ConfigFiles;
using DocFxTocGenerator.Test.Helpers;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace DocFxTocGenerator.Test;

public class ConfigFilesServiceTests
{
    private Faker _faker = new();
    private MockFileService _fileService = new();
    private MockLogger _mockLogger = new();
    private ILogger _logger;
    private readonly ITestOutputHelper _outputHelper;

    public ConfigFilesServiceTests(ITestOutputHelper outputHelper)
    {
        _outputHelper = outputHelper;
        _fileService.FillDemoSet();
        _logger = _mockLogger.Logger;
    }

    [Fact]
    public void OrderList_GetExistingFile_ShouldBeValid()
    {
        // arrange
        ConfigFilesService service = new(camelCasing: false, _fileService, _logger);

        // act
        var mockfile = _fileService.GetOrderFile();
        var folderPath = Path.GetDirectoryName(mockfile.Path)!;

        var list = service.GetOrderList(folderPath);

        // assert
        list.Should().NotBeEmpty();
        // expect list + 2 defaults
        var lines = mockfile.Content.Split("\n");
        list.Should().HaveCount(lines.Length + 2);
        list[0].Should().Be("index");
        list[1].Should().Be("readme");
        int index = 2;
        foreach (var line in lines)
        {
            list[index++].Should().Be(line);
        }
    }

    [Fact]
    public void OrderList_GetNonExisting_ShouldBeInitialized()
    {
        // arrange
        ConfigFilesService service = new(camelCasing: false, _fileService, _logger);
        string folder = _fileService.AddFolder("temp");

        // act
        var list = service.GetOrderList(folder);

        // cleanup
        _fileService.Delete(folder);

        // assert
        list.Should().NotBeEmpty();
        // expect list + 2 defaults
        list.Should().HaveCount(2);
        list[0].Should().Be("index");
        list[1].Should().Be("readme");
    }

    [Fact]
    public void OrderList_GetExistingWithIndex_ShouldBeOrderedCorrectly()
    {
        // arrange
        ConfigFilesService service = new(camelCasing: false, _fileService, _logger);
        string folder = _fileService.AddFolder("temp");
        _fileService.AddFile(folder, ".order",
@"readme
number-one");

        // act
        var list = service.GetOrderList(folder);

        // cleanup
        _fileService.Delete("temp/.order");
        _fileService.Delete(folder);

        // assert
        list.Should().NotBeEmpty();
        // expect list + 2 defaults
        list.Should().HaveCount(3);
        list[0].Should().Be("index");
        list[1].Should().Be("readme");
        list[2].Should().Be("number-one");
    }

    [Fact]
    public void IgnoreList_GetExistingFile_ShouldBeValid()
    {
        // arrange
        ConfigFilesService service = new(camelCasing: false, _fileService, _logger);

        // act
        var mockfile = _fileService.GetIgnoreFile();
        var folderPath = Path.GetDirectoryName(mockfile.Path)!;

        var list = service.GetIgnoreList(folderPath);

        // assert
        var lines = mockfile.Content.Split("\n");
        list.Should().NotBeEmpty();
        list.Should().HaveCount(lines.Length);
        int index = 0;
        foreach (var entry in list)
        {
            entry.Should().Be(lines[index++]);
        }
    }

    [Fact]
    public void IgnoreList_GetNonExisting_ShouldBeInitialized()
    {
        // arrange
        ConfigFilesService service = new(camelCasing: false, _fileService, _logger);
        string folder = _fileService.AddFolder("temp");

        // act
        var list = service.GetIgnoreList(folder);

        // cleanup
        _fileService.Delete(folder);

        // assert
        list.Should().BeEmpty();
    }

    [Fact]
    public void OverrideList_GetExistingFile_ShouldBeValid()
    {
        // arrange
        ConfigFilesService service = new(camelCasing: false, _fileService, _logger);

        // act
        var mockfile = _fileService.GetOverrideFile();
        var folderPath = Path.GetDirectoryName(mockfile.Path)!;

        var list = service.GetOverrideList(folderPath);

        // assert
        var lines = mockfile.Content.Split("\n");
        list.Should().NotBeEmpty();
        list.Should().HaveCount(lines.Length);
        int index = 0;
        foreach (var entry in list)
        {
            var parts = lines[index++].Split(';');
            entry.Key.Should().Be(parts[0]);
            entry.Value.Should().Be(parts[1]);
        }
    }

    [Fact]
    public void OverrideList_GetNonExisting_ShouldBeInitialized()
    {
        // arrange
        ConfigFilesService service = new(camelCasing: false, _fileService, _logger);
        string folder = _fileService.AddFolder("temp");

        // act
        var list = service.GetOverrideList(folder);

        // cleanup
        _fileService.Delete(folder);

        // assert
        list.Should().BeEmpty();
    }
}
