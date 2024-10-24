// <copyright file="TableOfContentsServiceTests.cs" company="DocFx Companion Tools">
// Copyright (c) DocFx Companion Tools. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
using System.CodeDom.Compiler;
using Bogus;
using DocFxTocGenerator.Actions;
using DocFxTocGenerator.ConfigFiles;
using DocFxTocGenerator.FileService;
using DocFxTocGenerator.TableOfContents;
using DocFxTocGenerator.Test.Helpers;
using FluentAssertions;
using Microsoft.Extensions.Logging;

namespace DocFxTocGenerator.Test;

public class TableOfContentsServiceTests
{
    private Faker _faker = new();
    private MockFileService _fileService = new();
    private MockLogger _mockLogger = new();
    private ILogger _logger;
    private ConfigFilesService _config;

    public TableOfContentsServiceTests()
    {
        _fileService.FillDemoSet();
        _logger = _mockLogger.Logger;
        _config = new(camelCasing: false, _fileService, _logger);
    }

    [Fact]
    public async Task GetTocItems_GetTocForFolderWithOrdering()
    {
        // arrange
        ContentInventoryAction action = new(_fileService.Root, useOrder: true, useIgnore: true, useOverride: true, camelCasing: false, _fileService, _logger);
        TableOfContentsService service = new(_fileService.Root, TocFolderReferenceStrategy.None, TocOrderStrategy.All, _fileService, _logger);
        await action.RunAsync();
        FolderData? current = action!.RootFolder!.Find("continents/americas/brasil");

        // act
        var toc = service.GetTocItemsForFolder(current!, 0);

        // assert
        toc.Should().NotBeNull();
        toc.IsFolder.Should().BeTrue();
        toc.Depth.Should().Be(0);
        toc.Base.Should().Be(current);
        toc.Name.Should().Be("Brasil");
        toc.Sequence.Should().Be(current!.Sequence);
        toc.Href.Should().BeNull();

        toc.Items.Count.Should().Be(3);
        // validate ordering defined for this folder in .order file. See MockFileService.
        toc.Items[0].Name.Should().Be("Sao Paulo");
        toc.Items[1].Name.Should().Be("Nova Friburgo");
        toc.Items[2].Name.Should().Be("Rio de Janeiro");
    }

    [Fact]
    public async Task GetTocItems_GetTocForFolderWithIgnore()
    {
        // arrange
        ContentInventoryAction action = new(_fileService.Root, useOrder: true, useIgnore: true, useOverride: true, camelCasing: false, _fileService, _logger);
        TableOfContentsService service = new(_fileService.Root, TocFolderReferenceStrategy.None, TocOrderStrategy.All, _fileService, _logger);
        await action.RunAsync();
        FolderData? current = action!.RootFolder!.Find("continents/americas/united-states");

        // act
        var toc = service.GetTocItemsForFolder(current!, 1);

        // assert
        toc.Should().NotBeNull();
        toc.IsFolder.Should().BeTrue();
        toc.Depth.Should().Be(1);
        toc.Base.Should().Be(current);
        toc.Name.Should().Be("United states");
        toc.Sequence.Should().Be(current!.Sequence);
        toc.Href.Should().BeNull();

        // validate folder 'texas' is ignored (from .ignore file)
        toc.Items.Count.Should().Be(3);
        // validate standard ordering by name
        toc.Items[0].Name.Should().Be("California");
        toc.Items[1].Name.Should().Be("New york");
        toc.Items[2].Name.Should().Be("Washington");
    }

    [Fact]
    public async Task GetTocItems_GetTocForFolderWithOverride()
    {
        // arrange
        ContentInventoryAction action = new(_fileService.Root, useOrder: true, useIgnore: true, useOverride: true, camelCasing: false, _fileService, _logger);
        TableOfContentsService service = new(_fileService.Root, TocFolderReferenceStrategy.None, TocOrderStrategy.All, _fileService, _logger);
        await action.RunAsync();
        FolderData? current = action!.RootFolder!.Find("continents/americas/united-states/washington");

        // act
        var toc = service.GetTocItemsForFolder(current!, 2);

        // assert
        toc.Should().NotBeNull();
        toc.IsFolder.Should().BeTrue();
        toc.Depth.Should().Be(2);
        toc.Base.Should().Be(current);
        toc.Name.Should().Be("Washington");
        toc.Sequence.Should().Be(current!.Sequence);
        toc.Href.Should().BeNull();

        toc.Items.Count.Should().Be(2);
        // validate standard ordering by name
        toc.Items[0].Name.Should().Be("Seattle");
        // validate Tacoma is overridden with other text from .override
        toc.Items[1].Name.Should().Be("This is where the airport is - Tacoma Airport");
    }

    [Fact]
    public async Task GetTocItems_GetTocFolderReferenceIndex()
    {
        // arrange
        ContentInventoryAction action = new(_fileService.Root, useOrder: true, useIgnore: true, useOverride: true, camelCasing: false, _fileService, _logger);
        TableOfContentsService service = new(_fileService.Root, TocFolderReferenceStrategy.Index, TocOrderStrategy.All, _fileService, _logger);
        await action.RunAsync();
        FolderData? current = action!.RootFolder!.Find("deep-tree/level1");

        // act
        var toc = service.GetTocItemsForFolder(current!, 3);

        // assert
        toc.Should().NotBeNull();
        toc.IsFolder.Should().BeTrue();
        toc.Depth.Should().Be(3);
        toc.Base.Should().Be(current);
        toc.Name.Should().Be("Level1");
        toc.Sequence.Should().Be(current!.Sequence);
        toc.Href.Should().BeNull();

        toc.Items.Count.Should().Be(1);
        toc.Items[0].Name.Should().Be("Index of LEVEL 2");
        toc.Items[0].Href.Should().Be("deep-tree/level1/level2/index.md");
    }

    [Fact]
    public async Task GetTocItems_GetTocFolderReferenceReadme()
    {
        // arrange
        ContentInventoryAction action = new(_fileService.Root, useOrder: true, useIgnore: true, useOverride: true, camelCasing: false, _fileService, _logger);
        TableOfContentsService service = new(_fileService.Root, TocFolderReferenceStrategy.IndexReadme, TocOrderStrategy.All, _fileService, _logger);
        await action.RunAsync();
        FolderData? current = action!.RootFolder!.Find("deep-tree/level1/level2/level3/level4");

        // act
        var toc = service.GetTocItemsForFolder(current!, 4);

        // assert
        toc.Should().NotBeNull();
        toc.IsFolder.Should().BeTrue();
        toc.Depth.Should().Be(4);
        toc.Base.Should().Be(current);
        toc.Name.Should().Be("Level4");
        toc.Sequence.Should().Be(current!.Sequence);
        toc.Href.Should().BeNull();

        toc.Items.Count.Should().Be(1);
        toc.Items[0].Name.Should().Be("Deep tree readme");
        toc.Items[0].Href.Should().Be("deep-tree/level1/level2/level3/level4/level5/README.md");
    }

    [Fact]
    public async Task GetTocItems_GetTocFolderReferenceFirst()
    {
        // arrange
        ContentInventoryAction action = new(_fileService.Root, useOrder: true, useIgnore: true, useOverride: true, camelCasing: false, _fileService, _logger);
        TableOfContentsService service = new(_fileService.Root, TocFolderReferenceStrategy.First, TocOrderStrategy.All, _fileService, _logger);
        await action.RunAsync();
        FolderData? current = action!.RootFolder!.Find("continents/europe/netherlands");

        // act
        var toc = service.GetTocItemsForFolder(current!, 5);

        // assert
        toc.Should().NotBeNull();
        toc.IsFolder.Should().BeTrue();
        toc.Depth.Should().Be(5);
        toc.Base.Should().Be(current);
        toc.Name.Should().Be("Netherlands");
        toc.Sequence.Should().Be(current!.Sequence);
        toc.Href.Should().BeNull();

        toc.Items.Count.Should().Be(2);
        // validate it picked the first file entry, ordered on display name (Rotterdam, The Hague)
        toc.Items[1].Name.Should().Be("Rotterdam");
        toc.Items[1].Href.Should().Be("continents/europe/netherlands/zuid-holland/rotterdam.md");
    }

    [Fact]
    public async Task GetTocItems_GetTocOrderingAll()
    {
        // arrange
        ContentInventoryAction action = new(_fileService.Root, useOrder: true, useIgnore: true, useOverride: true, camelCasing: false, _fileService, _logger);
        TableOfContentsService service = new(_fileService.Root, TocFolderReferenceStrategy.None, TocOrderStrategy.All, _fileService, _logger);
        await action.RunAsync();
        FolderData? current = action!.RootFolder!.Find("continents");

        // act
        var toc = service.GetTocItemsForFolder(current!, 6);

        // assert
        toc.Should().NotBeNull();
        toc.IsFolder.Should().BeTrue();
        toc.Depth.Should().Be(6);
        toc.Base.Should().Be(current);
        toc.Name.Should().Be("Continents");
        toc.Sequence.Should().Be(current!.Sequence);
        toc.Href.Should().BeNull();

        toc.Items.Count.Should().Be(4);
        toc.Items[0].Name.Should().Be("Continents README");
        toc.Items[1].Name.Should().Be("Americas");
        toc.Items[2].Name.Should().Be("Europe");
        toc.Items[3].Name.Should().Be("Unmentioned Continents");
    }

    [Fact]
    public async Task GetTocItems_GetTocOrderingFoldersFirst()
    {
        // arrange
        ContentInventoryAction action = new(_fileService.Root, useOrder: true, useIgnore: true, useOverride: true, camelCasing: false, _fileService, _logger);
        TableOfContentsService service = new(_fileService.Root, TocFolderReferenceStrategy.None, TocOrderStrategy.FoldersFirst, _fileService, _logger);
        await action.RunAsync();
        FolderData? current = action!.RootFolder!.Find("continents");

        // act
        var toc = service.GetTocItemsForFolder(current!, 6);

        // assert
        toc.Should().NotBeNull();
        toc.IsFolder.Should().BeTrue();
        toc.Depth.Should().Be(6);
        toc.Base.Should().Be(current);
        toc.Name.Should().Be("Continents");
        toc.Sequence.Should().Be(current!.Sequence);
        toc.Href.Should().BeNull();

        toc.Items.Count.Should().Be(4);
        toc.Items[0].Name.Should().Be("Americas");
        toc.Items[1].Name.Should().Be("Europe");
        toc.Items[2].Name.Should().Be("Continents README");
        toc.Items[3].Name.Should().Be("Unmentioned Continents");
    }

    [Fact]
    public async Task GetTocItems_GetTocOrderingFilesFirst()
    {
        // arrange
        ContentInventoryAction action = new(_fileService.Root, useOrder: true, useIgnore: true, useOverride: true, camelCasing: false, _fileService, _logger);
        TableOfContentsService service = new(_fileService.Root, TocFolderReferenceStrategy.None, TocOrderStrategy.FilesFirst, _fileService, _logger);
        await action.RunAsync();
        FolderData? current = action!.RootFolder!.Find("continents");

        // act
        var toc = service.GetTocItemsForFolder(current!, 6);

        // assert
        toc.Should().NotBeNull();
        toc.IsFolder.Should().BeTrue();
        toc.Depth.Should().Be(6);
        toc.Base.Should().Be(current);
        toc.Name.Should().Be("Continents");
        toc.Sequence.Should().Be(current!.Sequence);
        toc.Href.Should().BeNull();

        toc.Items.Count.Should().Be(4);
        toc.Items[0].Name.Should().Be("Continents README");
        toc.Items[1].Name.Should().Be("Unmentioned Continents");
        toc.Items[2].Name.Should().Be("Americas");
        toc.Items[3].Name.Should().Be("Europe");
    }

    [Fact]
    public async Task SerializeTocItem_OneLevelOnly()
    {
        // arrange
        ContentInventoryAction action = new(_fileService.Root, useOrder: true, useIgnore: true, useOverride: true, camelCasing: false, _fileService, _logger);
        TableOfContentsService service = new(_fileService.Root, TocFolderReferenceStrategy.None, TocOrderStrategy.FilesFirst, _fileService, _logger);

        using StringWriter sw = new StringWriter();
        using IndentedTextWriter writer = new IndentedTextWriter(sw, "  ");

        await action.RunAsync();
        var toc = service.GetTocItemsForFolder(action.RootFolder!, 0);

        string expected =
@"- name: Docs
- name: Main readme
  href: README.md
- name: Continents
  href: continents/toc.yml
- name: Deep tree
  href: deep-tree/toc.yml
- name: Software
  href: software/toc.yml
";

        // act
        service.SerializeTocItem(writer, toc, maxDepth: 1);

        // assert
        string output = sw.ToString();
        output.Should().Be(expected);
    }

    [Fact]
    public async Task SerializeTocItem_OneLevelOnly_WithStartPath()
    {
        // arrange
        ContentInventoryAction action = new(_fileService.Root, useOrder: true, useIgnore: true, useOverride: true, camelCasing: false, _fileService, _logger);
        TableOfContentsService service = new(_fileService.Root, TocFolderReferenceStrategy.None, TocOrderStrategy.FilesFirst, _fileService, _logger);

        using StringWriter sw = new StringWriter();
        using IndentedTextWriter writer = new IndentedTextWriter(sw, "  ");

        await action.RunAsync();
        string rootPath = "continents/americas";
        var current = action.RootFolder!.Find(rootPath);
        var toc = service.GetTocItemsForFolder(current!, 0);

        // expected links have relative paths to files and folders in rootFolder
        string expected =
@"- name: Americas
- name: The Americas
  href: README.md
- name: Brasil
  href: brasil/toc.yml
- name: United states
  href: united-states/toc.yml
";

        // act
        service.SerializeTocItem(writer, toc, maxDepth: 1, startPath: rootPath);

        // assert
        string output = sw.ToString();
        output.Should().Be(expected);
    }

    [Fact]
    public async Task SerializeTocItem_Hierarchy()
    {
        // arrange
        ContentInventoryAction action = new(_fileService.Root, useOrder: true, useIgnore: true, useOverride: true, camelCasing: false, _fileService, _logger);
        TableOfContentsService service = new(_fileService.Root, TocFolderReferenceStrategy.None, TocOrderStrategy.FilesFirst, _fileService, _logger);

        using StringWriter sw = new StringWriter();
        using IndentedTextWriter writer = new IndentedTextWriter(sw, "  ");

        await action.RunAsync();
        var toc = service.GetTocItemsForFolder(action.RootFolder!, 0);

        #region expected output
        string expected =
@"- name: Docs
- name: Main readme
  href: README.md
- name: Continents
  items:
  - name: Continents README
    href: continents/README.md
  - name: Unmentioned Continents
    href: continents/unmentioned-continents.md
  - name: Americas
    items:
    - name: The Americas
      href: continents/americas/README.md
    - name: Brasil
      items:
      - name: Sao Paulo
        href: continents/americas/brasil/sao-paulo.md
      - name: Nova Friburgo
        href: continents/americas/brasil/nova-friburgo.md
      - name: Rio de Janeiro
        href: continents/americas/brasil/rio-de-janeiro.md
    - name: United states
      items:
      - name: California
        items:
        - name: Los Angeles
          href: continents/americas/united-states/california/los-angeles.md
        - name: San Diego
          href: continents/americas/united-states/california/san-diego.md
        - name: San Francisco
          href: continents/americas/united-states/california/san-francisco.md
      - name: New york
        items:
        - name: New York City
          href: continents/americas/united-states/new-york/new-york-city.md
      - name: Washington
        items:
        - name: Seattle
          href: continents/americas/united-states/washington/seattle.md
        - name: This is where the airport is - Tacoma Airport
          href: continents/americas/united-states/washington/tacoma.md
  - name: Europe
    items:
    - name: Europe
      href: continents/europe/README.md
    - name: Germany
      items:
      - name: Germany README
        href: continents/europe/germany/README.md
      - name: Berlin
        href: continents/europe/germany/berlin.md
      - name: München
        href: continents/europe/germany/munchen.md
    - name: Netherlands
      items:
      - name: Noord holland
        items:
        - name: Amsterdam
          href: continents/europe/netherlands/noord-holland/amsterdam.md
      - name: Zuid holland
        items:
        - name: Rotterdam
          href: continents/europe/netherlands/zuid-holland/rotterdam.md
        - name: The Hague
          href: continents/europe/netherlands/zuid-holland/den-haag.md
- name: Deep tree
  items:
  - name: Level1
    items:
    - name: Level2
      items:
      - name: Index of LEVEL 2
        href: deep-tree/level1/level2/index.md
      - name: Level3
        items:
        - name: Level4
          items:
          - name: Level5
            items:
            - name: Deep tree readme
              href: deep-tree/level1/level2/level3/level4/level5/README.md
- name: Software
  items:
  - name: Apis
    items:
    - name: Test api
      items:
      - name: Feature.proto 1.0.0.3145
        href: software/apis/test-api/test-api.swagger.json
    - name: Test plain api
      items:
      - name: SimpleApi.Test
        href: software/apis/test-plain-api/swagger.json
";
        #endregion

        // act
        service.SerializeTocItem(writer, toc, maxDepth: 0);

        // assert
        string output = sw.ToString();
        output.Should().Be(expected);
    }

    [Fact]
    public async Task SerializeTocItem_Hierarchy_CamelCase()
    {
        // arrange
        ContentInventoryAction action = new(_fileService.Root, useOrder: true, useIgnore: true, useOverride: true, camelCasing: true, _fileService, _logger);
        TableOfContentsService service = new(_fileService.Root, TocFolderReferenceStrategy.None, TocOrderStrategy.FilesFirst, _fileService, _logger);

        using StringWriter sw = new StringWriter();
        using IndentedTextWriter writer = new IndentedTextWriter(sw, "  ");

        await action.RunAsync();
        var toc = service.GetTocItemsForFolder(action.RootFolder!, 0);

        #region expected output
        string expected =
@"- name: docs
- name: main readme
  href: README.md
- name: continents
  items:
  - name: continents README
    href: continents/README.md
  - name: unmentioned Continents
    href: continents/unmentioned-continents.md
  - name: americas
    items:
    - name: the Americas
      href: continents/americas/README.md
    - name: brasil
      items:
      - name: sao Paulo
        href: continents/americas/brasil/sao-paulo.md
      - name: nova Friburgo
        href: continents/americas/brasil/nova-friburgo.md
      - name: rio de Janeiro
        href: continents/americas/brasil/rio-de-janeiro.md
    - name: united states
      items:
      - name: california
        items:
        - name: los Angeles
          href: continents/americas/united-states/california/los-angeles.md
        - name: san Diego
          href: continents/americas/united-states/california/san-diego.md
        - name: san Francisco
          href: continents/americas/united-states/california/san-francisco.md
      - name: new york
        items:
        - name: new York City
          href: continents/americas/united-states/new-york/new-york-city.md
      - name: washington
        items:
        - name: seattle
          href: continents/americas/united-states/washington/seattle.md
        - name: This is where the airport is - Tacoma Airport
          href: continents/americas/united-states/washington/tacoma.md
  - name: europe
    items:
    - name: europe
      href: continents/europe/README.md
    - name: germany
      items:
      - name: germany README
        href: continents/europe/germany/README.md
      - name: berlin
        href: continents/europe/germany/berlin.md
      - name: münchen
        href: continents/europe/germany/munchen.md
    - name: netherlands
      items:
      - name: noord holland
        items:
        - name: amsterdam
          href: continents/europe/netherlands/noord-holland/amsterdam.md
      - name: zuid holland
        items:
        - name: rotterdam
          href: continents/europe/netherlands/zuid-holland/rotterdam.md
        - name: the Hague
          href: continents/europe/netherlands/zuid-holland/den-haag.md
- name: deep tree
  items:
  - name: level1
    items:
    - name: level2
      items:
      - name: index of LEVEL 2
        href: deep-tree/level1/level2/index.md
      - name: level3
        items:
        - name: level4
          items:
          - name: level5
            items:
            - name: deep tree readme
              href: deep-tree/level1/level2/level3/level4/level5/README.md
- name: software
  items:
  - name: apis
    items:
    - name: test api
      items:
      - name: feature.proto 1.0.0.3145
        href: software/apis/test-api/test-api.swagger.json
    - name: test plain api
      items:
      - name: simpleApi.Test
        href: software/apis/test-plain-api/swagger.json
";
        #endregion

        // act
        service.SerializeTocItem(writer, toc, maxDepth: 0);

        // assert
        string output = sw.ToString();
        output.Should().Be(expected);
    }

    [Fact]
    public async Task WriteTocFile_OneLevel()
    {
        // arrange
        ContentInventoryAction action = new(_fileService.Root, useOrder: true, useIgnore: true, useOverride: true, camelCasing: false, _fileService, _logger);
        TableOfContentsService service = new(_fileService.Root, TocFolderReferenceStrategy.None, TocOrderStrategy.FilesFirst, _fileService, _logger);

        await action.RunAsync();
        var toc = service.GetTocItemsForFolder(action.RootFolder!, 0);

        string expected =
@"# This is an automatically generated file
- name: Docs
- name: Main readme
  href: README.md
- name: Continents
  href: continents/toc.yml
- name: Deep tree
  href: deep-tree/toc.yml
- name: Software
  href: software/toc.yml
";

        // act
        await service.WriteTocFileAsync(toc, maxDepth: 1);

        // assert
        string tocPath = Path.Combine(_fileService.Root, "toc.yml");
        _fileService.ExistsFileOrDirectory(tocPath).Should().BeTrue();
        string output = _fileService.ReadAllText(tocPath);
        output.Should().Be(expected);
    }
}
