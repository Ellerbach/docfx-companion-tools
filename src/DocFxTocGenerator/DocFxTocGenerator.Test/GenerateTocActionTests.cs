// <copyright file="GenerateTocActionTests.cs" company="DocFx Companion Tools">
// Copyright (c) DocFx Companion Tools. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
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
        ContentInventoryAction content = new(_fileService.Root, useOrder: true, useIgnore: true, useOverride: true, camelCasing: false, _fileService, _logger);
        await content.RunAsync();

        EnsureIndexAction index = new(content.RootFolder!, Index.IndexGenerationStrategy.Never, camelCasing: false, _fileService, _logger);
        await index.RunAsync();

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
        ReturnCode ret = await action.RunAsync();

        // assert
        ret.Should().Be(ReturnCode.Normal);
        _fileService.Files.Should().HaveCount(originalCount + 1);
    }

    [Fact]
    public async void Run_WithDepth1()
    {
        // arrange
        _fileService.Files.Clear();
        _fileService.AddFile(string.Empty, "README.md", string.Empty.AddHeading("Multitoc", 1).AddParagraphs(2));
        string folder = _fileService.AddFolder("continents");
        _fileService.AddFile(folder, "index.md", string.Empty.AddHeading("Continents", 1).AddParagraphs(1));

        folder = _fileService.AddFolder("continents/americas");
        _fileService.AddFile(folder, "README.md", string.Empty.AddHeading("Americas", 1).AddParagraphs(1));
        _fileService.AddFile(folder, "extra-facts.md", string.Empty.AddHeading("Americas Extra Facts", 1).AddParagraphs(1));
        folder = _fileService.AddFolder("continents/americas/brasil");
        _fileService.AddFile(folder, "README.md", string.Empty.AddHeading("Brasil", 1).AddParagraphs(1));
        _fileService.AddFile(folder, "nova-friburgo.md", string.Empty.AddHeading("Nova Friburgo", 1).AddParagraphs(1));
        _fileService.AddFile(folder, "rio-de-janeiro.md", string.Empty.AddHeading("Rio de Janeiro", 1).AddParagraphs(1));
        folder = _fileService.AddFolder("continents/americas/united-states");
        _fileService.AddFile(folder, "los-angeles.md", string.Empty.AddHeading("Los Angeles", 1).AddParagraphs(1));
        _fileService.AddFile(folder, "new-york.md", string.Empty.AddHeading("New York", 1).AddParagraphs(1));
        _fileService.AddFile(folder, "washington.md", string.Empty.AddHeading("Washington", 1).AddParagraphs(1));

        folder = _fileService.AddFolder("continents/europe");
        _fileService.AddFile(folder, "README.md", string.Empty.AddHeading("Europe", 1).AddParagraphs(1));
        folder = _fileService.AddFolder("continents/europe/germany");
        _fileService.AddFile(folder, "berlin.md", string.Empty.AddHeading("Berlin", 1).AddParagraphs(1));
        _fileService.AddFile(folder, "munich.md", string.Empty.AddHeading("Munich", 1).AddParagraphs(1));
        folder = _fileService.AddFolder("continents/europe/netherlands");
        _fileService.AddFile(folder, "amsterdam.md", string.Empty.AddHeading("Amsterdam", 1).AddParagraphs(1));
        _fileService.AddFile(folder, "rotterdam.md", string.Empty.AddHeading("Rotterdam", 1).AddParagraphs(1));

        folder = _fileService.AddFolder("vehicles");
        _fileService.AddFile(folder, "index.md", string.Empty.AddHeading("Vehicles", 1).AddParagraphs(1));
        folder = _fileService.AddFolder("vehicles/cars");
        _fileService.AddFile(folder, "audi.md", string.Empty.AddHeading("Audi", 1).AddParagraphs(1));
        _fileService.AddFile(folder, "bmw.md", string.Empty.AddHeading("BMW", 1).AddParagraphs(1));
        _fileService.AddFile(folder, "README.md", string.Empty.AddHeading("Cars", 1).AddParagraphs(1));

        ContentInventoryAction content = new(_fileService.Root, useOrder: false, useIgnore: false, useOverride: false, camelCasing: false, _fileService, _logger);
        await content.RunAsync();

        EnsureIndexAction index = new(content.RootFolder!, Index.IndexGenerationStrategy.Never, camelCasing: false, _fileService, _logger);
        await index.RunAsync();

        GenerateTocAction action = new(
            _fileService.Root,
            content.RootFolder!,
            folderReferenceStrategy: TocFolderReferenceStrategy.None,
            orderStrategy: TocOrderStrategy.All,
            maxDepth: 1,
            _fileService,
            _logger);

        string rootExpected =
@"# This is an automatically generated file
- name: Docs
- name: Multitoc
  href: README.md
- name: Continents
  href: continents/toc.yml
- name: Vehicles
  href: vehicles/toc.yml
";

        string continentsExpected =
@"# This is an automatically generated file
- name: Continents
  href: index.md
- name: Americas
  items:
  - name: Americas
    href: americas/README.md
  - name: Americas Extra Facts
    href: americas/extra-facts.md
  - name: Brasil
    items:
    - name: Brasil
      href: americas/brasil/README.md
    - name: Nova Friburgo
      href: americas/brasil/nova-friburgo.md
    - name: Rio de Janeiro
      href: americas/brasil/rio-de-janeiro.md
  - name: United states
    items:
    - name: Los Angeles
      href: americas/united-states/los-angeles.md
    - name: New York
      href: americas/united-states/new-york.md
    - name: Washington
      href: americas/united-states/washington.md
- name: Europe
  items:
  - name: Europe
    href: europe/README.md
  - name: Germany
    items:
    - name: Berlin
      href: europe/germany/berlin.md
    - name: Munich
      href: europe/germany/munich.md
  - name: Netherlands
    items:
    - name: Amsterdam
      href: europe/netherlands/amsterdam.md
    - name: Rotterdam
      href: europe/netherlands/rotterdam.md
";

        string vehiclesExpected =
@"# This is an automatically generated file
- name: Vehicles
  href: index.md
- name: Cars
  items:
  - name: Cars
    href: cars/README.md
  - name: Audi
    href: cars/audi.md
  - name: BMW
    href: cars/bmw.md
";

        int originalCount = _fileService.Files.Count();

        // act
        ReturnCode ret = await action.RunAsync();

        // assert
        ret.Should().Be(ReturnCode.Normal);

        string toc = _fileService.ReadAllText(_fileService.GetFullPath("toc.yml"));
        toc.Should().Be(rootExpected);

        toc = _fileService.ReadAllText(_fileService.GetFullPath("continents/toc.yml"));
        toc.Should().Be(continentsExpected);

        toc = _fileService.ReadAllText(_fileService.GetFullPath("vehicles/toc.yml"));
        toc.Should().Be(vehiclesExpected);
    }

    [Fact]
    public async void Run_Issue_1()
    {
        // arrange
        _fileService.Files.Clear();
        _fileService.AddFile(string.Empty, "index.md", string.Empty.AddHeading("Index", 1).AddParagraphs(1));
        _fileService.AddFile(string.Empty, "file1.md", string.Empty.AddHeading("File 1", 1).AddParagraphs(1));
        _fileService.AddFile(string.Empty, "file2.md", string.Empty.AddHeading("File 2", 1).AddParagraphs(1));

        ContentInventoryAction content = new(_fileService.Root, useOrder: false, useIgnore: false, useOverride: false, camelCasing: false, _fileService, _logger);
        await content.RunAsync();

        EnsureIndexAction index = new(content.RootFolder!, Index.IndexGenerationStrategy.Never, camelCasing: false, _fileService, _logger);
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
- name: Index
  href: index.md
- name: File 1
  href: file1.md
- name: File 2
  href: file2.md
";

        // act
        ReturnCode ret = await action.RunAsync();

        // assert
        ret.Should().Be(ReturnCode.Normal);
        _fileService.Files.Should().HaveCount(originalCount + 1);
        string toc = _fileService.ReadAllText(_fileService.GetFullPath("toc.yml"));
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

        ContentInventoryAction content = new(_fileService.Root, useOrder: false, useIgnore: false, useOverride: false, camelCasing: false, _fileService, _logger);
        await content.RunAsync();

        EnsureIndexAction index = new(content.RootFolder!, Index.IndexGenerationStrategy.NotExists, camelCasing: false, _fileService, _logger);
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
- name: Index
  href: index.md
- name: A
  href: A/index.md
  items:
  - name: B doc
    href: A/B.md
";

        // act
        ReturnCode ret = await action.RunAsync();

        // assert
        ret.Should().Be(ReturnCode.Normal);
        _fileService.Files.Should().HaveCount(originalCount + 1);
        string toc = _fileService.ReadAllText(_fileService.GetFullPath("toc.yml"));
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

        ContentInventoryAction content = new(_fileService.Root, useOrder: false, useIgnore: false, useOverride: false, camelCasing: false, _fileService, _logger);
        await content.RunAsync();

        EnsureIndexAction index = new(content.RootFolder!, Index.IndexGenerationStrategy.NotExists, camelCasing: false, _fileService, _logger);
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
- name: Index
  href: index.md
- name: Index of A
  href: A/index.md
  items:
  - name: B doc
    href: A/B.md
";

        // act
        ReturnCode ret = await action.RunAsync();

        // assert
        ret.Should().Be(ReturnCode.Normal);
        _fileService.Files.Should().HaveCount(originalCount + 1);
        string toc = _fileService.ReadAllText(_fileService.GetFullPath("toc.yml"));
        toc.Should().Be(expected);
    }
}
