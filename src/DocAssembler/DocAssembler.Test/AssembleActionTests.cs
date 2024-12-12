// <copyright file="AssembleActionTests.cs" company="DocFx Companion Tools">
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

public class AssembleActionTests
{
    private Faker _faker = new();
    private MockFileService _fileService = new();
    private MockLogger _mockLogger = new();
    private ILogger _logger;

    private string _workingFolder = string.Empty;
    private string _outputFolder = string.Empty;

    public AssembleActionTests()
    {
        _fileService.FillDemoSet();
        _logger = _mockLogger.Logger;

        _workingFolder = _fileService.Root;
        _outputFolder = Path.Combine(_fileService.Root, "out");
    }

    [Fact]
    public async void Run_MinimumConfigAllCopied()
    {
        // arrange
        AssembleConfiguration config = new AssembleConfiguration
        {
            DestinationFolder = "out",
            Content =
            [
                new Content
                    {
                        SourceFolder = ".docfx",
                        Files = { "**" },
                        RawCopy = true,         // just copy the content
                    }
                ],
        };

        // all files in .docfx and docs-children
        int count = _fileService.Files.Count;
        var expected = _fileService.Files.Where(x => x.Key.Contains("/.docfx/"));

        InventoryAction inventory = new(_workingFolder, config, _fileService, _logger);
        await inventory.RunAsync();
        AssembleAction action = new(config, inventory.Files, _fileService, _logger);

        // act
        var ret = await action.RunAsync();

        // assert
        ret.Should().Be(ReturnCode.Normal);
        // expect files to be original count + expected files and folders + new "out" folder.
        _fileService.Files.Should().HaveCount(count + expected.Count() + 1);

        // validate file content is copied
        var file = expected.Single(x => x.Key.EndsWith("index.md"));
        var expectedPath = file.Key.Replace("/.docfx/", $"/{config.DestinationFolder}/");
        var newFile = _fileService.Files.SingleOrDefault(x => x.Key == expectedPath);
        newFile.Should().NotBeNull();
        newFile.Value.Should().Be(file.Value);
    }

    [Fact]
    public async void Run_MinimumConfigAllCopied_WithGlobalContentReplace()
    {
        // arrange
        AssembleConfiguration config = new AssembleConfiguration
        {
            DestinationFolder = "out",
            ContentReplacements =
            [
                new Replacement
                {
                    Expression = @"(?<pre>[$\s])AB#(?<id>[0-9]{3,6})",
                    Value = @"${pre}[AB#${id}](https://dev.azure.com/MyCompany/MyProject/_workitems/edit/${id})"
                }
            ],
            Content =
            [
                new Content
                    {
                        SourceFolder = "docs",
                        DestinationFolder = "general",
                        Files = { "**" },
                    }
                ],
        };

        // all files in .docfx and docs-children
        int count = _fileService.Files.Count;
        var expected = _fileService.Files.Where(x => x.Key.StartsWith($"{_fileService.Root}/docs/"));

        InventoryAction inventory = new(_workingFolder, config, _fileService, _logger);
        await inventory.RunAsync();
        AssembleAction action = new(config, inventory.Files, _fileService, _logger);

        // act
        var ret = await action.RunAsync();

        // assert
        ret.Should().Be(ReturnCode.Normal);
        // expect files to be original count + expected files and folders + new "out" folder.
        _fileService.Files.Should().HaveCount(count + expected.Count() + 1);

        // validate file content is copied with changed AB# reference
        var file = expected.Single(x => x.Key == $"{_fileService.Root}/docs/guidelines/documentation-guidelines.md");
        var expectedPath = file.Key.Replace("/docs/", $"/{config.DestinationFolder}/general/");
        var newFile = _fileService.Files.SingleOrDefault(x => x.Key == expectedPath);
        newFile.Should().NotBeNull();
        string[] lines = _fileService.ReadAllLines(newFile.Key);
        var testLine = lines.Single(x => x.StartsWith("STANDARD:"));
        testLine.Should().NotBeNull();
        testLine.Should().Be("STANDARD: [AB#1234](https://dev.azure.com/MyCompany/MyProject/_workitems/edit/1234) reference");
    }

    [Fact]
    public async void Run_MinimumConfigAllCopied_WithContentReplace()
    {
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
                        ContentReplacements =
                        [
                            new Replacement
                            {
                                Expression = @"(?<pre>[$\s])AB#(?<id>[0-9]{3,6})",
                                Value = @"${pre}[AB#${id}](https://dev.azure.com/MyCompany/MyProject/_workitems/edit/${id})"
                            }
                        ],
                    }
                ],
        };

        // all files in .docfx and docs-children
        int count = _fileService.Files.Count;
        var expected = _fileService.Files.Where(x => x.Key.StartsWith($"{_fileService.Root}/docs/"));

        InventoryAction inventory = new(_workingFolder, config, _fileService, _logger);
        await inventory.RunAsync();
        AssembleAction action = new(config, inventory.Files, _fileService, _logger);

        // act
        var ret = await action.RunAsync();

        // assert
        ret.Should().Be(ReturnCode.Normal);
        // expect files to be original count + expected files and folders + new "out" folder.
        _fileService.Files.Should().HaveCount(count + expected.Count() + 1);

        // validate file content is copied with changed AB# reference
        var file = expected.Single(x => x.Key == $"{_fileService.Root}/docs/guidelines/documentation-guidelines.md");
        var expectedPath = file.Key.Replace("/docs/", $"/{config.DestinationFolder}/general/");
        var newFile = _fileService.Files.SingleOrDefault(x => x.Key == expectedPath);
        newFile.Should().NotBeNull();
        string[] lines = _fileService.ReadAllLines(newFile.Key);
        var testLine = lines.Single(x => x.StartsWith("STANDARD:"));
        testLine.Should().NotBeNull();
        testLine.Should().Be("STANDARD: [AB#1234](https://dev.azure.com/MyCompany/MyProject/_workitems/edit/1234) reference");
    }

    [Fact]
    public async void Run_MinimumConfigAllCopied_ContentShouldOverrideGlobal()
    {
        // arrange
        AssembleConfiguration config = new AssembleConfiguration
        {
            DestinationFolder = "out",
            ContentReplacements =
            [
                new Replacement
                {
                    Expression = @"(?<pre>[$\s])AB#(?<id>[0-9]{3,6})",
                    Value = @"${pre}[AB#${id}](https://dev.azure.com/MyCompany/MyProject/_workitems/edit/${id})"
                }
            ],
            Content =
            [
                new Content
                    {
                        SourceFolder = "docs",
                        DestinationFolder = "general",
                        Files = { "**" },
                        ContentReplacements = [],
                    }
                ],
        };

        // all files in .docfx and docs-children
        int count = _fileService.Files.Count;
        var expected = _fileService.Files.Where(x => x.Key.StartsWith($"{_fileService.Root}/docs/"));

        InventoryAction inventory = new(_workingFolder, config, _fileService, _logger);
        await inventory.RunAsync();
        AssembleAction action = new(config, inventory.Files, _fileService, _logger);

        // act
        var ret = await action.RunAsync();

        // assert
        ret.Should().Be(ReturnCode.Normal);
        // expect files to be original count + expected files and folders + new "out" folder.
        _fileService.Files.Should().HaveCount(count + expected.Count() + 1);

        // validate file content is copied with changed AB# reference
        var file = expected.Single(x => x.Key == $"{_fileService.Root}/docs/guidelines/documentation-guidelines.md");
        var expectedPath = file.Key.Replace("/docs/", $"/{config.DestinationFolder}/general/");
        var newFile = _fileService.Files.SingleOrDefault(x => x.Key == expectedPath);
        newFile.Should().NotBeNull();
        string[] lines = _fileService.ReadAllLines(newFile.Key);
        var testLine = lines.Single(x => x.StartsWith("STANDARD:"));
        testLine.Should().NotBeNull();
        testLine.Should().Be("STANDARD: AB#1234 reference");
    }

    [Fact]
    public async void Run_StandardConfigAllCopied()
    {
        // arrange
        AssembleConfiguration config = GetStandardConfiguration();

        // all files in .docfx and docs-children
        int count = _fileService.Files.Count;
        var expected = _fileService.Files.Where(x => x.Key.Contains("/.docfx/") ||
                                                     x.Key.Contains("/docs/"));

        InventoryAction inventory = new(_workingFolder, config, _fileService, _logger);
        await inventory.RunAsync();
        AssembleAction action = new(config, inventory.Files, _fileService, _logger);

        // act
        var ret = await action.RunAsync();

        // assert
        ret.Should().Be(ReturnCode.Normal);
        // expect files to be original count + expected files and folders + new "out" folder.
        _fileService.Files.Should().HaveCount(count + expected.Count() + 1);

        // validate file content is copied with changed AB# reference
        var file = expected.Single(x => x.Key == $"{_fileService.Root}/docs/getting-started/README.md");
        var expectedPath = file.Key.Replace("/docs/", $"/{config.DestinationFolder}/general/");
        var newFile = _fileService.Files.SingleOrDefault(x => x.Key == expectedPath);
        newFile.Should().NotBeNull();

        string[] lines = _fileService.ReadAllLines(newFile.Key);

        string testLine = lines.Single(x => x.StartsWith("EXTERNAL:"));
        testLine.Should().Be("EXTERNAL: [.docassemble.json](https://github.com/example/blob/main/.docassemble.json)");

        testLine = lines.Single(x => x.StartsWith("RESOURCE:"));
        testLine.Should().Be("RESOURCE: ![computer](assets/computer.jpg)");

        testLine = lines.Single(x => x.StartsWith("PARENT-DOC:"));
        testLine.Should().Be("PARENT-DOC: [Docs readme](../README.md)");

        testLine = lines.Single(x => x.StartsWith("RELATIVE-DOC:"));
        testLine.Should().Be("RELATIVE-DOC: [Documentation guidelines](../guidelines/documentation-guidelines.md)");

        testLine = lines.Single(x => x.StartsWith("ANOTHER-DOCS-TREE:"));
        testLine.Should().Be("ANOTHER-DOCS-TREE: [System Copilot](../tools/system-copilot/README.md#usage)");

        testLine = lines.Single(x => x.StartsWith("ANOTHER-DOCS-TREE-BACKSLASH:"));
        testLine.Should().Be("ANOTHER-DOCS-TREE-BACKSLASH: [System Copilot](../tools/system-copilot/README.md#usage)");
    }

    private AssembleConfiguration GetStandardConfiguration()
    {
        return new AssembleConfiguration
        {
            DestinationFolder = "out",
            ExternalFilePrefix = "https://github.com/example/blob/main/",
            UrlReplacements =
            [
                new Replacement
                {
                    Expression = @"/[Dd]ocs/",
                    Value = "/"
                }
            ],
            ContentReplacements =
            [
                new Replacement
                {
                    Expression = @"(?<pre>[$\s])AB#(?<id>[0-9]{3,6})",
                    Value = @"${pre}[AB#${id}](https://dev.azure.com/MyCompany/MyProject/_workitems/edit/${id})"
                },
                new Replacement     // Remove markdown style table of content
                {
                    Expression = @"\[\[_TOC_\]\]",
                    Value = ""
                }
            ],
            Content =
            [
                new Content
                    {
                        SourceFolder = ".docfx",
                        Files = { "**" },
                        RawCopy = true,         // just copy the content
                        UrlReplacements = []    // reset URL replacements
                    },
                    new Content
                    {
                        SourceFolder = "docs",
                        DestinationFolder = "general",
                        Files = { "**" },
                    },
                    new Content
                    {
                        SourceFolder = "shared",    // part of general docs
                        DestinationFolder = "general/shared",
                        Files = { "**/docs/**" },
                    },
                    new Content
                    {
                        SourceFolder = "tools",     // part of general docs
                        DestinationFolder = "general/tools",
                        Files = { "**/docs/**" },
                    },
                    new Content
                    {
                        SourceFolder = "backend",
                        DestinationFolder = "services", // change name to services
                        Files = { "**/docs/**" },
                    },
                ],
        };
    }
}
