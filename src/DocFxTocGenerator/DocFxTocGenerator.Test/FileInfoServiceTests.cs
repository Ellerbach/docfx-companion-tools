// <copyright file="FileInfoServiceTests.cs" company="DocFx Companion Tools">
// Copyright (c) DocFx Companion Tools. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
using Bogus;
using DocFxTocGenerator.FileService;
using DocFxTocGenerator.Test.Helpers;
using FluentAssertions;
using Microsoft.Extensions.Logging;

namespace DocFxTocGenerator.Test;

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

    [Theory]
    [InlineData("file.md", "File.md", false)]
    [InlineData("something", "Something", false)]
    [InlineData("another-thing", "Another thing", false)]
    [InlineData("another--thing--with--double--dashes", "Another thing with double dashes", false)]
    [InlineData("a-file-with-strange-characters-\\/:*-END", "A file with strange characters END", false)]
    [InlineData("another file with áåàæßšűç Unicode", "Another file with áåàæßšűç Unicode", false)]
    [InlineData("Test of camelCasing.", "test of camelCasing.", true)]
    public void TitleCase_ShouldBeValid(string input, string expected, bool camelCasing)
    {
        // arrange
        FileInfoService service = new(camelCasing, _fileService, _logger);

        // act
        string output = service.ToTitleCase(input, camelCasing);

        // assert
        output.Should().Be(expected);
    }

    [Fact]
    public void CreateFileData_Defaults_ShouldCreateValidItem()
    {
        // arrange
        FileInfoService service = new(camelCasing: false, _fileService, _logger);
        string foldername = "MockFolder";
        string filename = "mock-file.md";
        FolderData? folder = new()
        {
            Name = foldername,
            DisplayName = "Mock folder",
            Path = _fileService.GetFullPath(foldername),
            Parent = null,
        };

        // act
        var file = service.CreateFileData(folder, filename);

        // assert
        file.Should().NotBeNull();
        file.Parent.Should().Be(folder);
        file.Name.Should().Be(filename);
        file.Path.NormalizePath().Should().Be(_fileService.GetFullPath(Path.Combine(foldername, filename)));
        file.DisplayName.Should().Be("Mock file");
        file.Sequence.Should().Be(int.MaxValue);
    }

    [Fact]
    public void CreateFileData_WithOrder_ShouldCreateValidItem()
    {
        // arrange
        FileInfoService service = new(camelCasing: false, _fileService, _logger);
        string foldername = "MockFolder";
        string filename = "mock-file.md";
        FolderData? folder = new()
        {
            Name = foldername,
            DisplayName = "Mockfolder",
            Path = _fileService.GetFullPath(foldername),
            Parent = null,
            OrderList = new() { "one", "two", Path.GetFileNameWithoutExtension(filename) },
        };

        // act
        var file = service.CreateFileData(folder, filename);

        // assert
        file.Should().NotBeNull();
        file.Parent.Should().Be(folder);
        file.Name.Should().Be(filename);
        file.Path.NormalizePath().Should().Be(_fileService.GetFullPath(Path.Combine(foldername, filename)));
        file.DisplayName.Should().Be("Mock file");
        file.Sequence.Should().Be(2);
    }

    [Fact]
    public void CreateFileData_WithOverride_ShouldCreateValidItem()
    {
        // arrange
        FileInfoService service = new(camelCasing: false, _fileService, _logger);
        string foldername = "MockFolder";
        string filename = "mock-file.md";
        string overridename = "Override name for this file.";
        FolderData? folder = new()
        {
            Name = foldername,
            DisplayName = "Mockfolder",
            Path = _fileService.GetFullPath(foldername),
            Parent = null,
            OverrideList = new() { { Path.GetFileNameWithoutExtension(filename), overridename } },
        };

        // act
        var file = service.CreateFileData(folder, filename);

        // assert
        file.Should().NotBeNull();
        file.Parent.Should().Be(folder);
        file.Name.Should().Be(filename);
        file.Path.NormalizePath().Should().Be(_fileService.GetFullPath(Path.Combine(foldername, filename)));
        file.DisplayName.Should().Be(overridename);
        file.Sequence.Should().Be(int.MaxValue);
    }

    [Fact]
    public void GetFileDisplayName_Markdown_GetValidH1()
    {
        // arrange
        FileInfoService service = new(camelCasing: false, _fileService, _logger);

        // act
        var title = service.GetFileDisplayName(_fileService.GetFullPath("continents/europe/germany/munchen.md"), false);

        // assert
        title.Should().Be("München");
    }

    [Fact]
    public void GetFileDisplayName_Markdown_GetValidH1_CamelCase()
    {
        // arrange
        FileInfoService service = new(camelCasing: false, _fileService, _logger);

        // act
        var title = service.GetFileDisplayName(_fileService.GetFullPath("continents/europe/germany/munchen.md"), true);

        // assert
        title.Should().Be("münchen");
    }

    [Fact]
    public void GetFileDisplayName_Markdown_GetValidH1_Complex()
    {
        // arrange
        FileInfoService service = new(camelCasing: false, _fileService, _logger);

        string folder = _fileService.AddFolder("temp");
        _fileService.AddFile(folder, "temp-markdown.md", string.Empty
            .AddHeading("`This is a header with quotes`", 1)
            .AddParagraphs(3));

        // act
        var title = service.GetFileDisplayName(_fileService.GetFullPath("temp/temp-markdown.md"), false);

        // cleanup
        _fileService.Delete("temp/temp-markdown.md");
        _fileService.Delete(folder);

        // assert
        title.Should().Be("This is a header with quotes");
    }

    [Fact]
    public void GetFileDisplayName_Markdown_GetValidH1_Complex_CamelCase()
    {
        // arrange
        FileInfoService service = new(camelCasing: true, _fileService, _logger);

        string folder = _fileService.AddFolder("temp");
        _fileService.AddFile(folder, "temp-markdown.md", string.Empty
            .AddHeading("`This is a header with quotes`", 1)
            .AddParagraphs(3));

        // act
        var title = service.GetFileDisplayName(_fileService.GetFullPath("temp/temp-markdown.md"), true);

        // cleanup
        _fileService.Delete("temp/temp-markdown.md");
        _fileService.Delete(folder);

        // assert
        title.Should().Be("this is a header with quotes");
    }

    [Fact]
    public void GetFileDisplayName_Markdown_GetFileNameWhenNoH1()
    {
        // arrange
        FileInfoService service = new(camelCasing: false, _fileService, _logger);

        string folder = _fileService.AddFolder("temp");
        _fileService.AddFile(folder, "temp-markdown.md", "A markdown file without H1");

        // act
        var title = service.GetFileDisplayName(_fileService.GetFullPath("temp/temp-markdown.md"), false);

        // cleanup
        _fileService.Delete("temp/temp-markdown.md");
        _fileService.Delete(folder);

        // assert
        title.Should().Be("Temp markdown");
    }

    [Fact]
    public void GetFileDisplayName_Markdown_GetFileNameWhenNoH1_CamelCase()
    {
        // arrange
        FileInfoService service = new(camelCasing: true, _fileService, _logger);

        string folder = _fileService.AddFolder("temp");
        _fileService.AddFile(folder, "temp-markdown.md", "A markdown file without H1");

        // act
        var title = service.GetFileDisplayName(_fileService.GetFullPath("temp/temp-markdown.md"), true);

        // cleanup
        _fileService.Delete("temp/temp-markdown.md");
        _fileService.Delete(folder);

        // assert
        title.Should().Be("temp markdown");
    }

    [Fact]
    public void GetFileDisplayName_NonExisting_ShouldNotCrash()
    {
        // arrange
        FileInfoService service = new(camelCasing: false, _fileService, _logger);

        // act
        var title = service.GetFileDisplayName("non-existing/test.md", false);

        // assert
        title.Should().Be("Test");
    }

    [Fact]
    public void GetFileDisplayName_NonExisting_ShouldNotCrash_CamelCase()
    {
        // arrange
        FileInfoService service = new(camelCasing: true, _fileService, _logger);

        // act
        var title = service.GetFileDisplayName("non-existing/test.md", true);

        // assert
        title.Should().Be("test");
    }

    [Fact]
    public void GetFileDisplayName_Swagger_GetValidH1()
    {
        // arrange
        FileInfoService service = new(camelCasing: false, _fileService, _logger);

        // act
        var title = service.GetFileDisplayName(_fileService.GetFullPath("software/apis/test-api/test-api.swagger.json"), false);

        // assert
        title.Should().Be("Feature.proto 1.0.0.3145");
    }

    [Fact]
    public void GetFileDisplayName_Swagger_GetValidH1_CamelCase()
    {
        // arrange
        FileInfoService service = new(camelCasing: true, _fileService, _logger);

        // act
        var title = service.GetFileDisplayName(_fileService.GetFullPath("software/apis/test-api/test-api.swagger.json"), true);

        // assert
        title.Should().Be("feature.proto 1.0.0.3145");
    }

    [Fact]
    public void GetFileDisplayName_Swagger_GetValidH1_MissingVersion()
    {
        // arrange
        FileInfoService service = new(camelCasing: false, _fileService, _logger);

        // act
        var title = service.GetFileDisplayName(_fileService.GetFullPath("software/apis/test-plain-api/swagger.json"), false);

        // assert
        title.Should().Be("SimpleApi.Test");
    }

    [Fact]
    public void GetFileDisplayName_Swagger_GetValidH1_MissingVersion_CamelCase()
    {
        // arrange
        FileInfoService service = new(camelCasing: true, _fileService, _logger);

        // act
        var title = service.GetFileDisplayName(_fileService.GetFullPath("software/apis/test-plain-api/swagger.json"), true);

        // assert
        title.Should().Be("simpleApi.Test");
    }

    [Fact]
    public void GetFileDisplayName_Swagger_GetFileNameWhenInvalidJson()
    {
        // arrange
        FileInfoService service = new(camelCasing: false, _fileService, _logger);

        string folder = _fileService.AddFolder("temp");
        _fileService.AddFile(folder, "temp.swagger.json", "This is invalid content for a json file.");

        // act
        var title = service.GetFileDisplayName(_fileService.GetFullPath("temp/temp.swagger.json"), false);

        // cleanup
        _fileService.Delete("temp/temp.swagger.json");
        _fileService.Delete(folder);

        // asserts
        title.Should().Be("Temp");
    }

    [Fact]
    public void GetFileDisplayName_Swagger_GetFileNameWhenInvalidJson_CamelCase()
    {
        // arrange
        FileInfoService service = new(camelCasing: true, _fileService, _logger);

        string folder = _fileService.AddFolder("temp");
        _fileService.AddFile(folder, "temp.swagger.json", "This is invalid content for a json file.");

        // act
        var title = service.GetFileDisplayName(_fileService.GetFullPath("temp/temp.swagger.json"), true);

        // cleanup
        _fileService.Delete("temp/temp.swagger.json");
        _fileService.Delete(folder);

        // asserts
        title.Should().Be("temp");
    }
}
