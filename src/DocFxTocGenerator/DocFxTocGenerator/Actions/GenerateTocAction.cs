// <copyright file="GenerateTocAction.cs" company="DocFx Companion Tools">
// Copyright (c) DocFx Companion Tools. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
using DocFxTocGenerator.FileService;
using DocFxTocGenerator.TableOfContents;
using Microsoft.Extensions.Logging;

namespace DocFxTocGenerator.Actions;

/// <summary>
/// Generate the table of contents for the given folder- and file-structure.
/// </summary>
public class GenerateTocAction
{
    private readonly string _outputFolder;
    private readonly FolderData _rootFolder = new();
    private readonly TocFolderReferenceStrategy _folderReferenceStrategy;
    private readonly TocOrderStrategy _orderStrategy;
    private readonly int _maxDepth;

    private readonly IFileService? _fileService;
    private readonly ILogger _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="GenerateTocAction"/> class.
    /// </summary>
    /// <param name="outputFolder">Output folder. This is optional.</param>
    /// <param name="rootFolder">Root folder with the content.</param>
    /// <param name="folderReferenceStrategy">Folder reference strategy.</param>
    /// <param name="orderStrategy">Order strategy.</param>
    /// <param name="maxDepth">Depth to generate a TOC for.</param>
    /// <param name="fileService">File service.</param>
    /// <param name="logger">Logger.</param>
    public GenerateTocAction(
        string outputFolder,
        FolderData rootFolder,
        TocFolderReferenceStrategy folderReferenceStrategy,
        TocOrderStrategy orderStrategy,
        int maxDepth,
        IFileService fileService,
        ILogger logger)
    {
        _outputFolder = outputFolder;
        _rootFolder = rootFolder;
        _maxDepth = maxDepth;
        _folderReferenceStrategy = folderReferenceStrategy;
        _orderStrategy = orderStrategy;

        _fileService = fileService;
        _logger = logger;
    }

    /// <summary>
    /// Run the action.
    /// </summary>
    /// <returns>0 on success, 1 on warning, 2 on error.</returns>
    public async Task<ReturnCode> RunAsync()
    {
        _logger.LogInformation($"\n*** GENERATE TABLE OF CONTENTS STAGE.");
        ReturnCode ret = ReturnCode.Normal;

        try
        {
            TableOfContentsService tocService = new(_outputFolder, _folderReferenceStrategy, _orderStrategy, _fileService!, _logger);

            // first get TOC hierarchy as objects
            TocItem toc = tocService.GetTocItemsForFolder(_rootFolder, 0);

            await tocService.WriteTocFileAsync(toc, _maxDepth);
        }
        catch (Exception ex)
        {
            _logger.LogCritical($"Write TOC error: {ex.Message}");
            ret = ReturnCode.Error;
        }

        _logger.LogInformation($"END OF GENERATE TABLE OF CONTENTS STATE. Result: {ret}");
        return ret;
    }
}
