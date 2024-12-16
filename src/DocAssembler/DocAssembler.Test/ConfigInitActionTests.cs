// <copyright file="ConfigInitActionTests.cs" company="DocFx Companion Tools">
// Copyright (c) DocFx Companion Tools. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
using Bogus;
using DocAssembler.Actions;
using DocAssembler.Configuration;
using DocAssembler.Test.Helpers;
using DocAssembler.Utils;
using FluentAssertions;
using Microsoft.Extensions.Logging;

namespace DocAssembler.Test;

public class ConfigInitActionTests
{
    private Faker _faker = new();
    private MockFileService _fileService = new();
    private MockLogger _mockLogger = new();
    private ILogger _logger;
    private string _outputFolder = string.Empty;

    public ConfigInitActionTests()
    {
        _fileService.FillDemoSet();
        _logger = _mockLogger.Logger;
        _outputFolder = Path.Combine(_fileService.Root, "out");
    }

    [Fact]
    public async void Run_ConfigShouldBeCreated()
    {
        // arrange
        ConfigInitAction action = new(_outputFolder, _fileService, _logger);
        int count = _fileService.Files.Count;

        // act
        var ret = await action.RunAsync();

        // assert
        ret.Should().Be(ReturnCode.Normal);
        _fileService.Files.Count.Should().Be(count + 1);

        // read generated content and see if it deserializes
        string content = _fileService.ReadAllText(_fileService.Files.Last().Key);
        var config = SerializationUtil.Deserialize<AssembleConfiguration>(content);
        config.Should().NotBeNull();
        config.DestinationFolder.Should().Be("out");
    }

    [Fact]
    public async void Run_ConfigShouldNotBeCreatedWhenExists()
    {
        // arrange
        ConfigInitAction action = new(_outputFolder, _fileService, _logger);
        var folder = _fileService.AddFolder(_outputFolder);
        var fileContents = "some content";
        _fileService.AddFile(folder, ".docassembler.json", fileContents);
        int count = _fileService.Files.Count;

        // act
        var ret = await action.RunAsync();

        // assert
        ret.Should().Be(ReturnCode.Error);
        _fileService.Files.Count.Should().Be(count); // nothing added

        // read file to see if still has the same content
        string content = _fileService.ReadAllText(_fileService.Files.Last().Key);
        content.Should().Be(fileContents);
    }
}
