// <copyright file="FileInfoServiceTests.cs" company="DocFx Companion Tools">
// Copyright (c) DocFx Companion Tools. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
using Bogus;
using DocAssembler.FileService;
using DocAssembler.Test.Helpers;
using FluentAssertions;
using Microsoft.Extensions.Logging;

namespace DocAssembler.Test;

public class FileInfoServiceTests
{
    private Faker _faker = new();
    private MockFileService _fileService = new();
    private MockLogger _mockLogger = new();
    private ILogger _logger;

    public FileInfoServiceTests()
    {
        _fileService.FillDemoSet();
        _logger = _mockLogger.Logger;
    }

    [Fact]
    public void GetLocalHyperlinks_GetAllWithoutResourceOrWeblink()
    {
        // arrange
        FileInfoService service = new(_fileService.Root, _fileService, _logger);

        // act
        var links = service.GetLocalHyperlinks("docs", "docs/getting-started/README.md");

        // assert
        links.Should().NotBeNull();
        links.Should().HaveCount(7);

        // testing a correction in our code, as the original is parsed weird by Markdig.
        // reason is that back-slashes in links are formally not supported.
        links[6].OriginalUrl.Should().Be(@"..\..\tools\system-copilot\docs\README.md#usage");
        links[6].Url.Should().Be(@"..\..\tools\system-copilot\docs\README.md#usage");
        links[6].UrlWithoutTopic.Should().Be(@"..\..\tools\system-copilot\docs\README.md");
        links[6].UrlTopic.Should().Be("#usage");
    }

    [Fact]
    public void GetLocalHyperlinks_SkipEmptyLink()
    {
        // arrange
        FileInfoService service = new(_fileService.Root, _fileService, _logger);

        // act
        var links = service.GetLocalHyperlinks("docs", "docs/guidelines/documentation-guidelines.md");

        // assert
        links.Should().NotBeNull();
        links.Should().BeEmpty();
    }

    [Fact]
    public void GetLocalHyperlinks_NotExistingFileThrows()
    {
        // arrange
        FileInfoService service = new(_fileService.Root, _fileService, _logger);

        // act
        Assert.Throws<FileNotFoundException>(() => _ = service.GetLocalHyperlinks("docs", "docs/not-existing/phantom-file.md"));
    }
}
