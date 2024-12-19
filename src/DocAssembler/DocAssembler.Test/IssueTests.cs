// <copyright file="IssueTests.cs" company="DocFx Companion Tools">
// Copyright (c) DocFx Companion Tools. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
using Bogus;
using DocAssembler.Actions;
using DocAssembler.Configuration;
using DocAssembler.Test.Helpers;
using FluentAssertions;
using Microsoft.Extensions.Logging;

namespace DocAssembler.Test;

public class IssueTests
{
    private Faker _faker = new();
    private MockFileService _fileService = new();
    private MockLogger _mockLogger = new();
    private ILogger _logger;

    private string _workingFolder = string.Empty;
    private string _outputFolder = string.Empty;

    public IssueTests()
    {
        _fileService.FillDemoSet();
        _logger = _mockLogger.Logger;

        _workingFolder = _fileService.Root;
        _outputFolder = Path.Combine(_fileService.Root, "out");
    }

    [Fact]
    public async void Issue_89_refHeaderInSameFile()
    {
        _fileService.Files.Clear();

        string expected =
@"#Documentation Readme

LINK [title](#documentation-readme)";

        var folder = _fileService.AddFolder($"docs");
        _fileService.AddFile(folder, "README.md", string.Empty
            .AddRaw(expected));

        // arrange
        AssembleConfiguration config = new AssembleConfiguration
        {
            DestinationFolder = "out",
            Content =
            [
                new Content
                {
                    SourceFolder = "docs",
                    DestinationFolder = "general",
                    Files = { "**" },
                }
            ]
        };

        InventoryAction action = new(_workingFolder, config, _fileService, _logger);

        // act
        var ret = await action.RunAsync();

        // assert
        ret.Should().Be(ReturnCode.Normal);
        var content = _fileService.ReadAllText(_fileService.Files.Last().Key);
        content.Should().Be(expected);
    }
}
