// <copyright file="EnsureIndexAction.cs" company="DocFx Companion Tools">
// Copyright (c) DocFx Companion Tools. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
using DocFxTocGenerator.FileService;
using DocFxTocGenerator.Index;
using Microsoft.Extensions.Logging;

namespace DocFxTocGenerator.Actions;

/// <summary>
/// Action to validate the content structure.
/// If there are empty folders, we will flag that as an error OR generate an index file, depending on the settings.
/// </summary>
public class EnsureIndexAction
{
    private readonly FolderData _rootFolder = new();
    private readonly IndexGenerationStrategy _indexGeneration;

    private readonly IFileService? _fileService;
    private readonly ILogger? _logger;
    private readonly IndexService _indexService;
    private readonly FileInfoService _fileDataService;

    /// <summary>
    /// Initializes a new instance of the <see cref="EnsureIndexAction"/> class.
    /// </summary>
    /// <param name="rootFolder">Root folder with the content.</param>
    /// <param name="indexGeneration">How to handle index generation.</param>
    /// <param name="fileService">File service.</param>
    /// <param name="logger">Logger.</param>
    public EnsureIndexAction(
        FolderData rootFolder,
        IndexGenerationStrategy indexGeneration,
        IFileService fileService,
        ILogger logger)
    {
        _rootFolder = rootFolder;
        _indexGeneration = indexGeneration;

        _fileService = fileService;
        _logger = logger;
        _indexService = new(fileService, logger);
        _fileDataService = new(fileService, logger);
    }

    /// <summary>
    /// Run the action.
    /// </summary>
    /// <returns>0 on success, 1 on warning, 2 on error.</returns>
    public Task<int> RunAsync()
    {
        _logger!.LogInformation($"\n*** ENSURE INDEX STAGE.");

        int ret = EnsureFolderIndex(_rootFolder);

        _logger!.LogInformation($"END OF ENSURE INDEX STAGE. Result: {ret}");
        return Task.FromResult(ret);
    }

    private int EnsureFolderIndex(FolderData folder)
    {
        _logger!.LogInformation($"Ensure folder index for '{folder.RelativePath}'");

        int ret = 0;

        // determine if we have to generate an index.
        bool generateIndex = _indexGeneration switch
        {
            IndexGenerationStrategy.Never => false,
            IndexGenerationStrategy.EmptyFolders => folder.FileCount == 0,
            IndexGenerationStrategy.NotExists => !folder.HasIndex,
            IndexGenerationStrategy.NotExistMultipleFiles => !folder.HasIndex && folder.FileCount != 1,
            _ => false,
        };

        if (folder.FileCount == 0 && !generateIndex)
        {
            // ERROR: no files in this folder.
            _logger!.LogError($"Folder `{folder.Path}` doesn't contain files. This cannot be handled when generating a table of contents. Use the --index flag to or add a file yourself.");
            ret = 2;
        }
        else
        {
            if (generateIndex && (folder.FileCount > 0 || folder.FolderCount > 0))
            {
                // We auto generate an index in this folder because of the settings.
                var indexPath = _indexService.GenerateIndex(_rootFolder, folder);
                if (!string.IsNullOrEmpty(indexPath))
                {
                    // generated index, so add to files to the list.
                    // get the index file from the order list (if added) to determine where to insert.
                    string? indexName = folder.OrderList.FirstOrDefault(x => string.Equals(x, "index", StringComparison.OrdinalIgnoreCase));
                    int index = Math.Min(folder.OrderList.IndexOf(indexName ?? "index"), folder.FileCount);
                    folder.Files.Insert(index < 0 ? 0 : index, _fileDataService.CreateFileData(folder, indexPath));
                }
                else
                {
                    // Error in generating the index.
                    ret = 2;
                }
            }
        }

        // now validate the subfolders
        foreach (var subfolder in folder.Folders)
        {
            int subRet = EnsureFolderIndex(subfolder);
            if (subRet > ret)
            {
                // make sure we record the highest error value.
                ret = subRet;
            }
        }

        return ret;
    }
}
