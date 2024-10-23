// <copyright file="TableOfContentsService.cs" company="DocFx Companion Tools">
// Copyright (c) DocFx Companion Tools. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
using System.CodeDom.Compiler;
using DocFxTocGenerator.FileService;
using Microsoft.Extensions.Logging;

namespace DocFxTocGenerator.TableOfContents;

/// <summary>
/// Service to generate the table of contents.
/// </summary>
public class TableOfContentsService
{
    private readonly string _outputFolder;
    private readonly TocFolderReferenceStrategy _folderReferenceStrategy;
    private readonly TocOrderStrategy _orderStrategy;
    private readonly IFileService _fileService;
    private readonly ILogger _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="TableOfContentsService"/> class.
    /// </summary>
    /// <param name="outputFolder">Output folder for the table of contents file.</param>
    /// <param name="folderReferenceStrategy">Folder reference strategy.</param>
    /// <param name="orderStrategy">Order strategy for table of content items.</param>
    /// <param name="fileService">File service.</param>
    /// <param name="logger">Logger.</param>
    public TableOfContentsService(
        string outputFolder,
        TocFolderReferenceStrategy folderReferenceStrategy,
        TocOrderStrategy orderStrategy,
        IFileService fileService,
        ILogger logger)
    {
        _outputFolder = outputFolder;
        _folderReferenceStrategy = folderReferenceStrategy;
        _orderStrategy = orderStrategy;
        _fileService = fileService;
        _logger = logger;
    }

    /// <summary>
    /// Add <see cref="TocItem"/> objects for folders and files in given folder to the given parent.
    /// </summary>
    /// <param name="folder">the toc item to serialize.</param>
    /// <param name="depth">Folder depth in the complete hierarchy.</param>
    /// <returns>Created table of contents item.</returns>
    public TocItem GetTocItemsForFolder(FolderData folder, int depth)
    {
        _logger!.LogInformation($"Get items for folder '{folder.RelativePath}' (depth {depth})");

        FileData? folderEntry = GetFolderEntry(folder);

        var tocItem = new TocItem
        {
            Name = folderEntry?.DisplayName ?? folder.DisplayName,
            Depth = depth,
            Sequence = folder.Sequence,
            Href = folderEntry?.RelativePath!,
            Base = folder,
        };

        // first add all sub folders
        foreach (var subfolder in folder.Folders)
        {
            tocItem.Items.Add(GetTocItemsForFolder(subfolder, depth + 1));
        }

        // then add all files
        foreach (var file in folder.Files)
        {
            if (file == folderEntry)
            {
                // we don't want to write the file used as entry-point for the folder twice.
                _logger.LogInformation($"  Skipping '{file.Name}' because it's already the entry-point for the folder.");
            }
            else
            {
                tocItem.Items.Add(new TocItem
                {
                    Name = file.DisplayName,
                    Depth = depth,
                    Href = file.RelativePath,
                    Sequence = file.Sequence,
                    Base = file,
                });
                _logger.LogInformation($"  Add file '{file.Name}' to the table of contents.");
            }
        }

        // order the list of items.
        tocItem.Items = _orderStrategy switch
        {
            TocOrderStrategy.All => tocItem.Items.OrderBy(x => x.Sequence).ThenBy(x => x.Name).ToList(),
            TocOrderStrategy.FoldersFirst => tocItem.Items.OrderByDescending(x => x.IsFolder).ThenBy(x => x.Sequence).ThenBy(x => x.Name).ToList(),
            TocOrderStrategy.FilesFirst => tocItem.Items.OrderByDescending(x => x.IsFile).ThenBy(x => x.Sequence).ThenBy(x => x.Name).ToList(),
            _ => tocItem.Items,
        };

        return tocItem;
    }

    /// <summary>
    /// Serialize a folder to the table of contents.
    /// </summary>
    /// <param name="writer">Writer to use for output.</param>
    /// <param name="root">the root toc item to serialize.</param>
    /// <param name="maxDepth">Maximum depth for TOCs. 0 = only the root.</param>
    /// <param name="startPath">Start path to cut. If left blank and we have a multi toc, it's calculated and passed.</param>
    /// <remarks>The representation is like this:
    /// items:
    /// - name: Sub Folder
    ///   href: sub-folder/index.md
    ///   items:
    ///   - name: Sub Document One
    ///     href: sub-folder/sub-document-one.md.
    /// - name: Document One
    ///   href: document-one.md
    /// - name: Document Two
    ///   href: document-two.md.
    /// </remarks>
    public void SerializeTocItem(IndentedTextWriter writer, TocItem root, int maxDepth, string startPath = "")
    {
        bool rootOnly = maxDepth != 0 && root.Depth < maxDepth;

        // If we have to build the TOC for muliple levels (maxDepth != 0),
        // each TOC references childs as relative to the current folder.
        // For that purpose, we calculate or pass on the root folder if we're
        // generating a hierarchy.
        string rootPath = startPath;
        int rootPathLength = 0;
        if (maxDepth != 0)
        {
            if (string.IsNullOrEmpty(startPath))
            {
                // first root to process, so determine the parent of the current level.
                rootPath = Path.GetDirectoryName(((FolderData)root.Base!).RelativePath)!;
            }

            // calculate the length to cut from the parent path + /
            rootPathLength = !string.IsNullOrEmpty(rootPath) ? rootPath.Length + 1 : 0;
        }

        // write the entry-point for the folder, only in highest level.
        if (rootOnly || root.Depth == 0)
        {
            writer.WriteLine($"- name: {root.Name}");
            if (!string.IsNullOrEmpty(root.Href))
            {
                writer.WriteLine($"  href: {root.Href.Substring(rootPathLength)}");
            }
        }

        if (root.Items.Count > 0)
        {
            foreach (var item in root.Items)
            {
                if (item.IsFolder && !rootOnly)
                {
                    writer.WriteLine($"- name: {item.Name}");
                    if (!string.IsNullOrEmpty(item.Href))
                    {
                        writer.WriteLine($"  href: {item.Href.Substring(rootPathLength)}");
                    }

                    writer.Indent++;
                    writer.WriteLine("items:");
                    SerializeTocItem(writer, item, maxDepth, rootPath);
                    writer.Indent--;
                }
                else
                {
                    string href = item.Href;
                    if (item.IsFolder && rootOnly)
                    {
                        // when we reference folders, we know we're referencing a new TOC level
                        // as this only happens in multi-toc mode. In that case, we always
                        // reference the toc file in that folder.
                        href = Path.Combine(((FolderData)item.Base!).RelativePath, "toc.yml").NormalizePath();
                    }

                    writer.WriteLine($"- name: {item.Name}");
                    if (!string.IsNullOrEmpty(href))
                    {
                        writer.WriteLine($"  href: {href.Substring(rootPathLength)}");
                    }
                }
            }
        }
    }

    /// <summary>
    /// Write a toc.yml file for the given table of contents. When maxDepth = 0 we create only a file
    /// in the root. When maxDepth > 0 we create a toc file on each depth below that maximum. On the
    /// highest levels it will be just indexing into the folders and files in the depth, on the lowest
    /// level it will be indexing into the complete tree from that level down. The option addParent is
    /// used in that case to optionally add a parent index item to enable an easy way to go back up the
    /// tree in the DocFX generated UI.
    /// </summary>
    /// <param name="toc">Table of contents item to generate the toc for.</param>
    /// <param name="maxDepth">Maximum depth to create TOC's for. 0 means, only 1 in the root.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task WriteTocFileAsync(TocItem toc, int maxDepth)
    {
        string subPath = toc.Depth > 0 ? toc.Base!.Path : string.Empty;

        // now write the hierarchy to a table of contents file.
        using StringWriter sw = new StringWriter();
        using IndentedTextWriter writer = new IndentedTextWriter(sw, "  ");
        await writer.WriteLineAsync("# This is an automatically generated file");

        SerializeTocItem(writer, toc, maxDepth, toc.Base!.RelativePath!);

        if (maxDepth > 0 && toc.Depth < maxDepth)
        {
            // we have to generate a TOC for the sub folders as well.
            foreach (var item in toc.Items)
            {
                if (item.IsFolder)
                {
                    await WriteTocFileAsync(item, maxDepth);
                }
            }
        }

        // now write the TOC to disc
        string outputFile = Path.Combine(_outputFolder, subPath, "toc.yml");
        _fileService.WriteAllText(outputFile, sw.ToString());

        _logger!.LogInformation($"{outputFile} created.");
    }

    private FileData? GetFolderEntry(FolderData folder)
    {
        FileData? folderEntry = null;

        if (_folderReferenceStrategy == TocFolderReferenceStrategy.None)
        {
            return null;
        }

        if (folder.HasIndex)
        {
            // index as entry-point
            folderEntry = folder.Index;
            _logger.LogInformation($"  Index file is entry-point for folder '{folder.Name}'");
        }
        else if (folder.HasReadme &&
                 (_folderReferenceStrategy == TocFolderReferenceStrategy.IndexReadme ||
                  _folderReferenceStrategy == TocFolderReferenceStrategy.First))
        {
            // readme as entry-point
            folderEntry = folder.Readme;
            _logger.LogInformation($"  Readme is entry-point for folder '{folder.Name}'");
        }
        else if (_folderReferenceStrategy == TocFolderReferenceStrategy.First && folder.Files.Count > 0)
        {
            // first file as entry-point
            folderEntry = folder.Files.First();
            _logger.LogInformation($"  First file '{folderEntry.Name}' is entry-point for folder '{folder.Name}'");
        }

        return folderEntry;
    }
}
