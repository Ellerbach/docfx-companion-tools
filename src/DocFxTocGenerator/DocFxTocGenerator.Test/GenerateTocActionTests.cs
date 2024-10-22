using Bogus;
using DocFxTocGenerator.Actions;
using DocFxTocGenerator.TableOfContents;
using DocFxTocGenerator.Test.Helpers;
using FluentAssertions;
using Microsoft.Extensions.Logging;

namespace DocFxTocGenerator.Test;

public class GenerateTocActionTests
{
    private Faker _faker = new();
    private MockFileService _fileService = new();
    private MockLogger _mockLogger = new();
    private ILogger _logger;

    public GenerateTocActionTests()
    {
        _fileService.FillDemoSet();
        _logger = _mockLogger.Logger;
    }

    [Fact]
    public async void Run_SimpleToc()
    {
        // arrange
        ContentInventoryAction content = new(_fileService.Root, useOrder: true, useIgnore: true, useOverride: true, _fileService, _logger);
        await content.RunAsync();

        EnsureIndexAction index = new(content.RootFolder!, Index.IndexGenerationStrategy.Never, _fileService, _logger);

        GenerateTocAction action = new(
            _fileService.Root,
            content.RootFolder!,
            folderReferenceStrategy: TocFolderReferenceStrategy.None,
            orderStrategy: TocOrderStrategy.All,
            maxDepth: 0,
            _fileService,
            _logger);

        int originalCount = _fileService.Files.Count();

        // act
        int ret = await action.RunAsync();

        // assert
        ret.Should().Be(0);
        _fileService.Files.Should().HaveCount(originalCount + 1);
    }

    [Fact]
    public async void Run_Issue_1()
    {
        // arrange
        _fileService.Files.Clear();
        _fileService.AddFile(string.Empty, "index.md", string.Empty.AddHeading("Index", 1).AddParagraphs(1));
        _fileService.AddFile(string.Empty, "file1.md", string.Empty.AddHeading("File 1", 1).AddParagraphs(1));
        _fileService.AddFile(string.Empty, "file2.md", string.Empty.AddHeading("File 2", 1).AddParagraphs(1));

        ContentInventoryAction content = new(_fileService.Root, useOrder: false, useIgnore: false, useOverride: false, _fileService, _logger);
        await content.RunAsync();

        EnsureIndexAction index = new(content.RootFolder!, Index.IndexGenerationStrategy.Never, _fileService, _logger);
        await index.RunAsync();

        GenerateTocAction action = new(
            _fileService.Root,
            content.RootFolder!,
            folderReferenceStrategy: TocFolderReferenceStrategy.First,
            orderStrategy: TocOrderStrategy.All,
            maxDepth: 0,
            _fileService,
            _logger);

        int originalCount = _fileService.Files.Count();

        string expected =
@"# This is an automatically generated file
- name: Docs
  href: index.md
- name: File 1
  href: file1.md
- name: File 2
  href: file2.md
";

        // act
        int ret = await action.RunAsync();

        // assert
        ret.Should().Be(0);
        _fileService.Files.Should().HaveCount(originalCount + 1);
        string toc = _fileService.ReadAllText(_fileService.GetFullPath("toc.yml")).Replace("\r", "");
        toc.Should().Be(expected);
    }

    [Fact]
    public async void Run_Issue_27_Run1()
    {
        // arrange
        _fileService.Files.Clear();
        _fileService.AddFile(string.Empty, "index.md", string.Empty.AddHeading("Index", 1).AddParagraphs(1));
        var folder = _fileService.AddFolder("A");
        _fileService.AddFile(folder, "B.md", string.Empty.AddHeading("B doc", 1).AddParagraphs(1));

        ContentInventoryAction content = new(_fileService.Root, useOrder: false, useIgnore: false, useOverride: false, _fileService, _logger);
        await content.RunAsync();

        EnsureIndexAction index = new(content.RootFolder!, Index.IndexGenerationStrategy.NotExists, _fileService, _logger);
        await index.RunAsync();

        GenerateTocAction action = new(
            _fileService.Root,
            content.RootFolder!,
            folderReferenceStrategy: TocFolderReferenceStrategy.First,
            orderStrategy: TocOrderStrategy.All,
            maxDepth: 0,
            _fileService,
            _logger);

        int originalCount = _fileService.Files.Count();

        string expected =
@"# This is an automatically generated file
- name: Docs
  href: index.md
- name: A
  href: A/index.md
  items:
  - name: B doc
    href: A/B.md
";

        // act
        int ret = await action.RunAsync();

        // assert
        ret.Should().Be(0);
        _fileService.Files.Should().HaveCount(originalCount + 1);
        string toc = _fileService.ReadAllText(_fileService.GetFullPath("toc.yml")).Replace("\r", "");
        toc.Should().Be(expected);
    }

    [Fact]
    public async void Run_Issue_27_Run2()
    {
        // arrange
        _fileService.Files.Clear();
        _fileService.AddFile(string.Empty, "index.md", string.Empty.AddHeading("Index", 1).AddParagraphs(1));
        var folder = _fileService.AddFolder("A");
        // in run 2 we simulate toc generator already run, so A/index.md already exists
        _fileService.AddFile(folder, "index.md", string.Empty.AddHeading("Index of A", 1).AddParagraphs(1));
        _fileService.AddFile(folder, "B.md", string.Empty.AddHeading("B doc", 1).AddParagraphs(1));

        ContentInventoryAction content = new(_fileService.Root, useOrder: false, useIgnore: false, useOverride: false, _fileService, _logger);
        await content.RunAsync();

        EnsureIndexAction index = new(content.RootFolder!, Index.IndexGenerationStrategy.NotExists, _fileService, _logger);
        await index.RunAsync();

        GenerateTocAction action = new(
            _fileService.Root,
            content.RootFolder!,
            folderReferenceStrategy: TocFolderReferenceStrategy.First,
            orderStrategy: TocOrderStrategy.All,
            maxDepth: 0,
            _fileService,
            _logger);

        int originalCount = _fileService.Files.Count();

        string expected =
@"# This is an automatically generated file
- name: Docs
  href: index.md
- name: A
  href: A/index.md
  items:
  - name: B doc
    href: A/B.md
";

        // act
        int ret = await action.RunAsync();

        // assert
        ret.Should().Be(0);
        _fileService.Files.Should().HaveCount(originalCount + 1);
        string toc = _fileService.ReadAllText(_fileService.GetFullPath("toc.yml")).Replace("\r", "");
        toc.Should().Be(expected);
    }
}
