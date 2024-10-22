using Bogus;
using DocFxTocGenerator.Actions;
using DocFxTocGenerator.Test.Helpers;
using FluentAssertions;
using Microsoft.Extensions.Logging;

namespace DocFxTocGenerator.Test;

public class EnsureIndecxActionTests
{
    private Faker _faker = new();
    private MockFileService _fileService = new();
    private ILogger _logger;

    public EnsureIndecxActionTests()
    {
        _fileService.FillDemoSet();
        _logger = MockLogger.GetMockedLogger();
    }

    [Fact]
    public async void Run_NoIndex()
    {
        // arrange
        ContentInventoryAction content = new(_fileService.Root, useOrder: false, useIgnore: false, useOverride: false, _fileService, _logger);
        await content.RunAsync();

        EnsureIndexAction action = new(content.RootFolder!, Index.IndexGenerationStrategy.Never, _fileService, _logger);
        int originalCount = _fileService.Files.Count;

        // act
        int ret = await action.RunAsync();

        // assert
        ret.Should().Be(0);
        _fileService.Files.Count.Should().Be(originalCount);
    }

    [Fact]
    public async void Run_GenerateForEmptyFolders()
    {
        // arrange
        ContentInventoryAction content = new(_fileService.Root, useOrder: false, useIgnore: false, useOverride: false, _fileService, _logger);
        await content.RunAsync();

        EnsureIndexAction action = new(content.RootFolder!, Index.IndexGenerationStrategy.EmptyFolders, _fileService, _logger);
        int originalCount = _fileService.Files.Count;

        // act
        int ret = await action.RunAsync();

        // assert
        ret.Should().Be(0);
        _fileService.Files.Count.Should().Be(originalCount + 8);
        int index = originalCount;
        _fileService.Files.ElementAt(index++).Key.Should().EndWith("continents/americas/united-states/index.md");
        _fileService.Files.ElementAt(index++).Key.Should().EndWith("continents/europe/netherlands/index.md");
        _fileService.Files.ElementAt(index++).Key.Should().EndWith("deep-tree/index.md");
        _fileService.Files.ElementAt(index++).Key.Should().EndWith("deep-tree/level1/index.md");
        _fileService.Files.ElementAt(index++).Key.Should().EndWith("deep-tree/level1/level2/level3/index.md");
        _fileService.Files.ElementAt(index++).Key.Should().EndWith("deep-tree/level1/level2/level3/level4/index.md");
        _fileService.Files.ElementAt(index++).Key.Should().EndWith("software/index.md");
        _fileService.Files.ElementAt(index++).Key.Should().EndWith("software/apis/index.md");
    }

    [Fact]
    public async void Run_GenerateForFoldersWithoutIndex()
    {
        // arrange
        ContentInventoryAction content = new(_fileService.Root, useOrder: false, useIgnore: false, useOverride: false, _fileService, _logger);
        await content.RunAsync();

        EnsureIndexAction action = new(content.RootFolder!, Index.IndexGenerationStrategy.NotExists, _fileService, _logger);
        int originalCount = _fileService.Files.Count;

        // act
        int ret = await action.RunAsync();

        // assert
        ret.Should().Be(0);
        _fileService.Files.Count.Should().Be(originalCount + 23);
        int index = originalCount;
        _fileService.Files.ElementAt(index++).Key.Should().EndWith("index.md");
        _fileService.Files.ElementAt(index++).Key.Should().EndWith("continents/index.md");
        _fileService.Files.ElementAt(index++).Key.Should().EndWith("continents/americas/index.md");
        _fileService.Files.ElementAt(index++).Key.Should().EndWith("continents/americas/brasil/index.md");
        _fileService.Files.ElementAt(index++).Key.Should().EndWith("continents/americas/united-states/index.md");
        _fileService.Files.ElementAt(index++).Key.Should().EndWith("continents/americas/united-states/california/index.md");
        _fileService.Files.ElementAt(index++).Key.Should().EndWith("continents/americas/united-states/new-york/index.md");
        _fileService.Files.ElementAt(index++).Key.Should().EndWith("continents/americas/united-states/texas/index.md");
        _fileService.Files.ElementAt(index++).Key.Should().EndWith("continents/americas/united-states/washington/index.md");
        _fileService.Files.ElementAt(index++).Key.Should().EndWith("continents/europe/index.md");
        _fileService.Files.ElementAt(index++).Key.Should().EndWith("continents/europe/germany/index.md");
        _fileService.Files.ElementAt(index++).Key.Should().EndWith("continents/europe/netherlands/index.md");
        _fileService.Files.ElementAt(index++).Key.Should().EndWith("continents/europe/netherlands/noord-holland/index.md");
        _fileService.Files.ElementAt(index++).Key.Should().EndWith("continents/europe/netherlands/zuid-holland/index.md");
        _fileService.Files.ElementAt(index++).Key.Should().EndWith("deep-tree/index.md");
        _fileService.Files.ElementAt(index++).Key.Should().EndWith("deep-tree/level1/index.md");
        _fileService.Files.ElementAt(index++).Key.Should().EndWith("deep-tree/level1/level2/level3/index.md");
        _fileService.Files.ElementAt(index++).Key.Should().EndWith("deep-tree/level1/level2/level3/level4/index.md");
        _fileService.Files.ElementAt(index++).Key.Should().EndWith("deep-tree/level1/level2/level3/level4/level5/index.md");
        _fileService.Files.ElementAt(index++).Key.Should().EndWith("software/index.md");
        _fileService.Files.ElementAt(index++).Key.Should().EndWith("software/apis/index.md");
    }

    [Fact]
    public async void Run_GenerateForFoldersWithoutIndexAndMultipleFiles()
    {
        // arrange
        ContentInventoryAction content = new(_fileService.Root, useOrder: false, useIgnore: false, useOverride: false, _fileService, _logger);
        await content.RunAsync();

        EnsureIndexAction action = new(content.RootFolder!, Index.IndexGenerationStrategy.NotExistMultipleFiles, _fileService, _logger);
        int originalCount = _fileService.Files.Count;

        // act
        int ret = await action.RunAsync();

        // assert
        ret.Should().Be(0);
        _fileService.Files.Count.Should().Be(originalCount + 14);
        int index = originalCount;
        _fileService.Files.ElementAt(index++).Key.Should().EndWith("continents/index.md");
        _fileService.Files.ElementAt(index++).Key.Should().EndWith("continents/americas/brasil/index.md");
        _fileService.Files.ElementAt(index++).Key.Should().EndWith("continents/americas/united-states/index.md");
        _fileService.Files.ElementAt(index++).Key.Should().EndWith("continents/americas/united-states/california/index.md");
        _fileService.Files.ElementAt(index++).Key.Should().EndWith("continents/americas/united-states/washington/index.md");
        _fileService.Files.ElementAt(index++).Key.Should().EndWith("continents/europe/germany/index.md");
        _fileService.Files.ElementAt(index++).Key.Should().EndWith("continents/europe/netherlands/index.md");
        _fileService.Files.ElementAt(index++).Key.Should().EndWith("continents/europe/netherlands/zuid-holland/index.md");
        _fileService.Files.ElementAt(index++).Key.Should().EndWith("deep-tree/index.md");
        _fileService.Files.ElementAt(index++).Key.Should().EndWith("deep-tree/level1/index.md");
        _fileService.Files.ElementAt(index++).Key.Should().EndWith("deep-tree/level1/level2/level3/index.md");
        _fileService.Files.ElementAt(index++).Key.Should().EndWith("deep-tree/level1/level2/level3/level4/index.md");
        _fileService.Files.ElementAt(index++).Key.Should().EndWith("software/index.md");
        _fileService.Files.ElementAt(index++).Key.Should().EndWith("software/apis/index.md");
    }

    [Fact]
    public async void Run_ErrorWithNullRoot()
    {
        // arrange
        EnsureIndexAction action = new(null, Index.IndexGenerationStrategy.Never, _fileService, _logger);

        // act
        int ret = await action.RunAsync();

        // assert
        ret.Should().Be(2);
    }
}
