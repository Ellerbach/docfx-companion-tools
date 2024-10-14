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
    private ILogger _logger;

    public FileInfoServiceTests()
    {
        _fileService.FillDemoSet();
        _logger = MockLogger.GetMockedLogger();
    }

    [Theory]
    [InlineData("file.md", "File.md")]
    [InlineData("something", "Something")]
    [InlineData("another-thing", "Another thing")]
    [InlineData("another--thing--with--double--dashes", "Another thing with double dashes")]
    [InlineData("a-file-with-strange-characters-\\/:*-END", "A file with strange characters END")]
    [InlineData("another file with áåàæßšűç Unicode", "Another file with áåàæßšűç Unicode")]
    public void TitleCase_ShouldBeValid(string input, string expected)
    {
        // arrange
        FileInfoService service = new(_fileService, _logger);

        // act
        string output = service.ToTitleCase(input);

        // assert
        output.Should().Be(expected);
    }

    [Fact]
    public void CreateFileData_Defaults_ShouldCreateValidItem()
    {
        // arrange
        FileInfoService service = new(_fileService, _logger);
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
        file.Path.Should().Be(_fileService.GetFullPath(Path.Combine(foldername, filename)));
        file.DisplayName.Should().Be("Mock file");
        file.Sequence.Should().Be(int.MaxValue);
    }

    [Fact]
    public void CreateFileData_WithOrder_ShouldCreateValidItem()
    {
        // arrange
        FileInfoService service = new(_fileService, _logger);
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
        file.Path.Should().Be(_fileService.GetFullPath(Path.Combine(foldername, filename)));
        file.DisplayName.Should().Be("Mock file");
        file.Sequence.Should().Be(2);
    }

    [Fact]
    public void CreateFileData_WithOverride_ShouldCreateValidItem()
    {
        // arrange
        FileInfoService service = new(_fileService, _logger);
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
        file.Path.Should().Be(_fileService.GetFullPath(Path.Combine(foldername, filename)));
        file.DisplayName.Should().Be(overridename);
        file.Sequence.Should().Be(int.MaxValue);
    }
}
