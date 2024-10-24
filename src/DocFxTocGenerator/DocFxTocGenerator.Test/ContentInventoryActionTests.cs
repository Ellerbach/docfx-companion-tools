// <copyright file="ContentInventoryActionTests.cs" company="DocFx Companion Tools">
// Copyright (c) DocFx Companion Tools. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
using Bogus;
using DocFxTocGenerator.Actions;
using DocFxTocGenerator.Test.Helpers;
using FluentAssertions;
using Microsoft.Extensions.Logging;

namespace DocFxTocGenerator.Test;

public class ContentInventoryActionTests
{
    private Faker _faker = new();
    private MockFileService _fileService = new();
    private MockLogger _mockLogger = new();
    private ILogger _logger;

    public ContentInventoryActionTests()
    {
        _fileService.FillDemoSet();
        _logger = _mockLogger.Logger;
    }

    [Fact]
    public async void Run_WithoutSettings()
    {
        // arrange
        ContentInventoryAction action = new(_fileService.Root, useOrder: false, useIgnore: false, useOverride: false, camelCasing: false, _fileService, _logger);

        // act
        ReturnCode ret = await action.RunAsync();

        // assert
        ret.Should().Be(ReturnCode.Normal);
        action.RootFolder.Should().NotBeNull();

        // root validation
        action.RootFolder.FolderCount.Should().Be(3);
        action.RootFolder.Folders[0].Name.Should().Be("continents");
        action.RootFolder.Folders[0].DisplayName.Should().Be("Continents");
        action.RootFolder.Folders[1].Name.Should().Be("deep-tree");
        action.RootFolder.Folders[1].DisplayName.Should().Be("Deep tree");
        action.RootFolder.Folders[2].Name.Should().Be("software");
        action.RootFolder.Folders[2].DisplayName.Should().Be("Software");
        action.RootFolder.FileCount.Should().Be(1);
        action.RootFolder.Files[0].Name.Should().Be("README.md");
        action.RootFolder.Files[0].DisplayName.Should().Be("Main readme");

        // one subfolder validation
        var current = action.RootFolder.Find("continents/europe");
        current.Should().NotBeNull();
        current.FolderCount.Should().Be(2);
        current.Folders[0].Name.Should().Be("germany");
        current.Folders[0].DisplayName.Should().Be("Germany");
        current.Folders[1].Name.Should().Be("netherlands");
        current.Folders[1].DisplayName.Should().Be("Netherlands");
        current.FileCount.Should().Be(1);
        current.Files[0].Name.Should().Be("README.md");
        current.Files[0].DisplayName.Should().Be($"Europe");

        // no order list
        var folderPath = Path.GetDirectoryName(_fileService.GetOrderFile().Path);
        current = action.RootFolder.Find(folderPath);
        current.OrderList.Should().HaveCount(2);    // only standard added items
        current = action.RootFolder.Find("continents/americas/brasil");
        current.Files.Should().HaveCount(3);
        // default sorting
        current.Files[0].Name.Should().Be("nova-friburgo.md");
        current.Files[0].DisplayName.Should().Be("Nova Friburgo");
        current.Files[1].Name.Should().Be("rio-de-janeiro.md");
        current.Files[1].DisplayName.Should().Be("Rio de Janeiro");
        current.Files[2].Name.Should().Be("sao-paulo.md");
        current.Files[2].DisplayName.Should().Be("Sao Paulo");

        // no ignore list
        folderPath = Path.GetDirectoryName(_fileService.GetIgnoreFile().Path);
        current = action.RootFolder.Find(folderPath);
        current.IgnoreList.Should().BeEmpty();
        current = action.RootFolder.Find("continents/americas/united-states");
        current.Folders.Should().HaveCount(4);
        var texas = current.Folders.SingleOrDefault(x => x.Name.Equals("texas", StringComparison.OrdinalIgnoreCase));
        // should not have been ignored
        texas.Should().NotBeNull();

        // no override list
        folderPath = Path.GetDirectoryName(_fileService.GetOverrideFile().Path);
        current = action.RootFolder.Find(folderPath);
        current.OverrideList.Should().BeEmpty();
        current = action.RootFolder.Find("continents/americas/united-states/washington");
        // no override used
        current.Files[1].DisplayName.Should().Be("Tacoma");
    }

    [Fact]
    public async void Run_WithOrderOnly()
    {
        // arrange
        ContentInventoryAction action = new(_fileService.Root, useOrder: true, useIgnore: false, useOverride: false, camelCasing: false, _fileService, _logger);

        // act
        ReturnCode ret = await action.RunAsync();

        // assert
        ret.Should().Be(ReturnCode.Normal);
        action.RootFolder.Should().NotBeNull();

        // root validation
        action.RootFolder.FolderCount.Should().Be(3);
        action.RootFolder.Folders[0].Name.Should().Be("continents");
        action.RootFolder.Folders[0].DisplayName.Should().Be("Continents");
        action.RootFolder.Folders[1].Name.Should().Be("deep-tree");
        action.RootFolder.Folders[1].DisplayName.Should().Be("Deep tree");
        action.RootFolder.Folders[2].Name.Should().Be("software");
        action.RootFolder.Folders[2].DisplayName.Should().Be("Software");
        action.RootFolder.FileCount.Should().Be(1);
        action.RootFolder.Files[0].Name.Should().Be("README.md");
        action.RootFolder.Files[0].DisplayName.Should().Be("Main readme");

        // one subfolder validation
        var current = action.RootFolder.Find("continents/europe");
        current.Should().NotBeNull();
        current.FolderCount.Should().Be(2);
        current.Folders[0].Name.Should().Be("germany");
        current.Folders[0].DisplayName.Should().Be("Germany");
        current.Folders[1].Name.Should().Be("netherlands");
        current.Folders[1].DisplayName.Should().Be("Netherlands");
        current.FileCount.Should().Be(1);
        current.Files[0].Name.Should().Be("README.md");
        current.Files[0].DisplayName.Should().Be($"Europe");

        // validate order list
        var folderPath = Path.GetDirectoryName(_fileService.GetOrderFile().Path);
        current = action.RootFolder.Find(folderPath);
        current.OrderList.Should().HaveCount(6);
        current = action.RootFolder.Find("continents/americas/brasil");
        current.Files.Should().HaveCount(3);
        // sorting from setings
        current.Files[0].Name.Should().Be("sao-paulo.md");
        current.Files[0].DisplayName.Should().Be("Sao Paulo");
        current.Files[1].Name.Should().Be("nova-friburgo.md");
        current.Files[1].DisplayName.Should().Be("Nova Friburgo");
        current.Files[2].Name.Should().Be("rio-de-janeiro.md");
        current.Files[2].DisplayName.Should().Be("Rio de Janeiro");


        // no ignore list
        folderPath = Path.GetDirectoryName(_fileService.GetIgnoreFile().Path);
        current = action.RootFolder.Find(folderPath);
        current.IgnoreList.Should().BeEmpty();
        current = action.RootFolder.Find("continents/americas/united-states");
        current.Folders.Should().HaveCount(4);
        var texas = current.Folders.SingleOrDefault(x => x.Name.Equals("texas", StringComparison.OrdinalIgnoreCase));
        // should not have been ignored
        texas.Should().NotBeNull();

        // no override list
        folderPath = Path.GetDirectoryName(_fileService.GetOverrideFile().Path);
        current = action.RootFolder.Find(folderPath);
        current.OverrideList.Should().BeEmpty();
        current = action.RootFolder.Find("continents/americas/united-states/washington");
        current.Files[1].DisplayName.Should().Be("Tacoma");
    }

    [Fact]
    public async void Run_WithIgnoreOnly()
    {
        // arrange
        ContentInventoryAction action = new(_fileService.Root, useOrder: false, useIgnore: true, useOverride: false, camelCasing: false, _fileService, _logger);

        // act
        ReturnCode ret = await action.RunAsync();

        // assert
        ret.Should().Be(ReturnCode.Normal);
        action.RootFolder.Should().NotBeNull();

        // root validation
        action.RootFolder.FolderCount.Should().Be(3);
        action.RootFolder.Folders[0].Name.Should().Be("continents");
        action.RootFolder.Folders[0].DisplayName.Should().Be("Continents");
        action.RootFolder.Folders[1].Name.Should().Be("deep-tree");
        action.RootFolder.Folders[1].DisplayName.Should().Be("Deep tree");
        action.RootFolder.Folders[2].Name.Should().Be("software");
        action.RootFolder.Folders[2].DisplayName.Should().Be("Software");
        action.RootFolder.FileCount.Should().Be(1);
        action.RootFolder.Files[0].Name.Should().Be("README.md");
        action.RootFolder.Files[0].DisplayName.Should().Be("Main readme");

        // one subfolder validation
        var current = action.RootFolder.Find("continents/europe");
        current.Should().NotBeNull();
        current.FolderCount.Should().Be(2);
        current.Folders[0].Name.Should().Be("germany");
        current.Folders[0].DisplayName.Should().Be("Germany");
        current.Folders[1].Name.Should().Be("netherlands");
        current.Folders[1].DisplayName.Should().Be("Netherlands");
        current.FileCount.Should().Be(1);
        current.Files[0].Name.Should().Be("README.md");
        current.Files[0].DisplayName.Should().Be($"Europe");

        // no order list
        var folderPath = Path.GetDirectoryName(_fileService.GetOrderFile().Path);
        current = action.RootFolder.Find(folderPath);
        current.OrderList.Should().HaveCount(2);    // only standard added items
        current = action.RootFolder.Find("continents/americas/brasil");
        current.Files.Should().HaveCount(3);
        // default sorting
        current.Files[0].Name.Should().Be("nova-friburgo.md");
        current.Files[0].DisplayName.Should().Be("Nova Friburgo");
        current.Files[1].Name.Should().Be("rio-de-janeiro.md");
        current.Files[1].DisplayName.Should().Be("Rio de Janeiro");
        current.Files[2].Name.Should().Be("sao-paulo.md");
        current.Files[2].DisplayName.Should().Be("Sao Paulo");

        // validate ignore list
        folderPath = Path.GetDirectoryName(_fileService.GetIgnoreFile().Path);
        current = action.RootFolder.Find(folderPath);
        current.IgnoreList.Should().HaveCount(1);
        current = action.RootFolder.Find("continents/americas/united-states");
        current.Folders.Should().HaveCount(3); // one ignored folder
        var texas = current.Folders.SingleOrDefault(x => x.Name.Equals("texas", StringComparison.OrdinalIgnoreCase));
        // should have been ignored
        texas.Should().BeNull();

        // no override list
        folderPath = Path.GetDirectoryName(_fileService.GetOverrideFile().Path);
        current = action.RootFolder.Find(folderPath);
        current.OverrideList.Should().BeEmpty();
        current = action.RootFolder.Find("continents/americas/united-states/washington");
        current.Files[1].DisplayName.Should().Be("Tacoma");
    }

    [Fact]
    public async void Run_WithOverrideOnly()
    {
        // arrange
        ContentInventoryAction action = new(_fileService.Root, useOrder: false, useIgnore: false, useOverride: true, camelCasing: false, _fileService, _logger);

        // act
        ReturnCode ret = await action.RunAsync();

        // assert
        ret.Should().Be(ReturnCode.Normal);
        action.RootFolder.Should().NotBeNull();

        // root validation
        action.RootFolder.FolderCount.Should().Be(3);
        action.RootFolder.Folders[0].Name.Should().Be("continents");
        action.RootFolder.Folders[0].DisplayName.Should().Be("Continents");
        action.RootFolder.Folders[1].Name.Should().Be("deep-tree");
        action.RootFolder.Folders[1].DisplayName.Should().Be("Deep tree");
        action.RootFolder.Folders[2].Name.Should().Be("software");
        action.RootFolder.Folders[2].DisplayName.Should().Be("Software");
        action.RootFolder.FileCount.Should().Be(1);
        action.RootFolder.Files[0].Name.Should().Be("README.md");
        action.RootFolder.Files[0].DisplayName.Should().Be("Main readme");

        // one subfolder validation
        var current = action.RootFolder.Find("continents/europe");
        current.Should().NotBeNull();
        current.FolderCount.Should().Be(2);
        current.Folders[0].Name.Should().Be("germany");
        current.Folders[0].DisplayName.Should().Be("Germany");
        current.Folders[1].Name.Should().Be("netherlands");
        current.Folders[1].DisplayName.Should().Be("Netherlands");
        current.FileCount.Should().Be(1);
        current.Files[0].Name.Should().Be("README.md");
        current.Files[0].DisplayName.Should().Be($"Europe");

        // no order list
        var folderPath = Path.GetDirectoryName(_fileService.GetOrderFile().Path);
        current = action.RootFolder.Find(folderPath);
        current.OrderList.Should().HaveCount(2);    // only standard added items
        current = action.RootFolder.Find("continents/americas/brasil");
        current.Files.Should().HaveCount(3);
        // default sorting
        current.Files[0].Name.Should().Be("nova-friburgo.md");
        current.Files[0].DisplayName.Should().Be("Nova Friburgo");
        current.Files[1].Name.Should().Be("rio-de-janeiro.md");
        current.Files[1].DisplayName.Should().Be("Rio de Janeiro");
        current.Files[2].Name.Should().Be("sao-paulo.md");
        current.Files[2].DisplayName.Should().Be("Sao Paulo");

        // no ignore list
        folderPath = Path.GetDirectoryName(_fileService.GetIgnoreFile().Path);
        current = action.RootFolder.Find(folderPath);
        current.IgnoreList.Should().BeEmpty();
        current = action.RootFolder.Find("continents/americas/united-states");
        current.Folders.Should().HaveCount(4);
        var texas = current.Folders.SingleOrDefault(x => x.Name.Equals("texas", StringComparison.OrdinalIgnoreCase));
        // should not have been ignored
        texas.Should().NotBeNull();

        // validate override list
        folderPath = Path.GetDirectoryName(_fileService.GetOverrideFile().Path);
        current = action.RootFolder.Find(folderPath);
        current.OverrideList.Should().HaveCount(1);
        current = action.RootFolder.Find("continents/americas/united-states/washington");
        current.Files[1].DisplayName.Should().Be("This is where the airport is - Tacoma Airport");
    }

    [Fact]
    public async void Run_WithNonExistingFolder_ReturnsError()
    {
        // arrange
        ContentInventoryAction action = new("x:\\Non-existing\\docs", useOrder: false, useIgnore: false, useOverride: true, camelCasing: false, _fileService, _logger);

        // act
        ReturnCode ret = await action.RunAsync();

        // assert
        ret.Should().Be(ReturnCode.Error);
        action.RootFolder.Should().BeNull();
    }


    [Fact]
    public async void Run_WithEmptyFolder_ReturnsWarning()
    {
        // arrange
        _fileService.Files.Clear();
        _fileService.AddFolder("x:\\Non-existing\\docs");
        ContentInventoryAction action = new("x:\\Non-existing\\docs", useOrder: false, useIgnore: false, useOverride: true, camelCasing: false, _fileService, _logger);

        // act
        ReturnCode ret = await action.RunAsync();

        // assert
        ret.Should().Be(ReturnCode.Warning);
        action.RootFolder.Should().BeNull();
    }
}
