// <copyright file="InventoryActionTests.cs" company="DocFx Companion Tools">
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

public class InventoryActionTests
{
    private Faker _faker = new();
    private MockFileService _fileService = new();
    private MockLogger _mockLogger = new();
    private ILogger _logger;

    private string _workingFolder = string.Empty;
    private string _outputFolder = string.Empty;

    public InventoryActionTests()
    {
        _fileService.FillDemoSet();
        _logger = _mockLogger.Logger;

        _workingFolder = _fileService.Root;
        _outputFolder = Path.Combine(_fileService.Root, "out");
    }

    [Fact]
    public async void Run_StandardConfigProducesExpectedFiles()
    {
        // arrange
        AssembleConfiguration config = GetStandardConfiguration();
        InventoryAction action = new(_workingFolder, config, _fileService, _logger);
        // all files in .docfx and docs-children
        var expected = _fileService.Files.Where(x => !string.IsNullOrEmpty(x.Value) &&
                                                     (x.Key.Contains("/.docfx/") || x.Key.Contains("/docs/")));

        // act
        var ret = await action.RunAsync();

        // assert
        ret.Should().Be(ReturnCode.Normal);
        action.Files.Should().HaveCount(expected.Count());
    }

    [Fact]
    public async void Run_MinimimalRawConfigProducesExpectedFiles()
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
        InventoryAction action = new(_workingFolder, config, _fileService, _logger);
        // all files in .docfx
        int expected = _fileService.Files.Count(x => !string.IsNullOrEmpty(x.Value) && x.Key.Contains("/.docfx/"));

        // act
        var ret = await action.RunAsync();

        // assert
        ret.Should().Be(ReturnCode.Normal);
        action.Files.Should().HaveCount(expected);
    }

    [Fact]
    public async void Run_MinimalRawConfigWithDoubleContent_ShouldFail()
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
                },
                new Content
                {
                    SourceFolder = ".docfx", // same content and destination should fail.
                    Files = { "**" },
                    RawCopy = true,
                }
            ],
        };
        InventoryAction action = new(_workingFolder, config, _fileService, _logger);

        // act
        var ret = await action.RunAsync();

        // assert
        ret.Should().Be(ReturnCode.Error);
    }

    [Fact]
    public async void Run_MinimalRawConfig_WithGlobalChangedPaths()
    {
        // arrange
        AssembleConfiguration config = new AssembleConfiguration
        {
            DestinationFolder = "out",
            UrlReplacements =
            [
                new Replacement
                {
                    Expression = @"/[Ii]mages/",
                    Value = "/assets/"
                }
            ],
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
        InventoryAction action = new(_workingFolder, config, _fileService, _logger);
        // all files in .docfx
        int expected = _fileService.Files.Count(x => !string.IsNullOrEmpty(x.Value) && x.Key.Contains("/.docfx/"));

        // act
        var ret = await action.RunAsync();

        // assert
        ret.Should().Be(ReturnCode.Normal);
        action.Files.Should().HaveCount(expected);
        var assets = action.Files.Where(x => x.SourcePath.Contains("/images"));
        assets.Should().HaveCount(1);

        string expectedPath = assets.First().SourcePath
            .Replace($"{_fileService.Root}/.docfx", $"{_fileService.Root}/{config.DestinationFolder}")
            .Replace("/images/", "/assets/");
        assets.First().DestinationPath.Should().Be(expectedPath);
    }

    [Fact]
    public async void Run_MinimalRawConfig_WithContentChangedPaths()
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
                        UrlReplacements =
                        [
                            new Replacement
                            {
                                Expression = @"/[Ii]mages/",
                                Value = "/assets/"
                            }
                        ],
                    }
                ],
        };
        InventoryAction action = new(_workingFolder, config, _fileService, _logger);
        // all files in .docfx
        int expected = _fileService.Files.Count(x => !string.IsNullOrEmpty(x.Value) && x.Key.Contains("/.docfx/"));

        // act
        var ret = await action.RunAsync();

        // assert
        ret.Should().Be(ReturnCode.Normal);
        action.Files.Should().HaveCount(expected);
        var assets = action.Files.Where(x => x.SourcePath.Contains("/images"));
        assets.Should().HaveCount(1);

        string expectedPath = assets.First().SourcePath
            .Replace($"{_fileService.Root}/.docfx", $"{_fileService.Root}/{config.DestinationFolder}")
            .Replace("/images/", "/assets/");
        assets.First().DestinationPath.Should().Be(expectedPath);
    }

    [Fact]
    public async void Run_MinimalRawConfig_WithContentOverruledNotChangedPaths()
    {
        // arrange
        AssembleConfiguration config = new AssembleConfiguration
        {
            DestinationFolder = "out",
            UrlReplacements =
            [
                new Replacement
                {
                    Expression = @"/[Ii]mages/",
                    Value = "/assets/"
                }
            ],
            Content =
            [
                new Content
                    {
                        SourceFolder = ".docfx",
                        Files = { "**" },
                        RawCopy = true,         // just copy the content
                        UrlReplacements = [],   // this overrides the global replacement
                    }
                ],
        };
        InventoryAction action = new(_workingFolder, config, _fileService, _logger);
        // all files in .docfx
        int expected = _fileService.Files.Count(x => !string.IsNullOrEmpty(x.Value) && x.Key.Contains("/.docfx/"));

        // act
        var ret = await action.RunAsync();

        // assert
        ret.Should().Be(ReturnCode.Normal);
        action.Files.Should().HaveCount(expected);
        var assets = action.Files.Where(x => x.SourcePath.Contains("/images"));
        assets.Should().HaveCount(1);

        string expectedPath = assets.First().SourcePath
            .Replace($"{_fileService.Root}/.docfx", $"{_fileService.Root}/{config.DestinationFolder}");
        assets.First().DestinationPath.Should().Be(expectedPath);
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
