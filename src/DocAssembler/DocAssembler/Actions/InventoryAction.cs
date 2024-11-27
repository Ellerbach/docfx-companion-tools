// <copyright file="InventoryAction.cs" company="DocFx Companion Tools">
// Copyright (c) DocFx Companion Tools. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
using System.Diagnostics;
using System.Text.RegularExpressions;
using DocAssembler.Configuration;
using DocAssembler.FileService;
using DocAssembler.Utils;
using Microsoft.Extensions.Logging;

namespace DocAssembler.Actions;

/// <summary>
/// Inventory action to retrieve configured content.
/// </summary>
public class InventoryAction
{
    private readonly string _workingFolder;
    private readonly string _outputFolder;
    private readonly FileInfoService _fileInfoService;
    private readonly IFileService _fileService;
    private readonly ILogger _logger;

    private readonly AssembleConfiguration _config;

    /// <summary>
    /// Initializes a new instance of the <see cref="InventoryAction"/> class.
    /// </summary>
    /// <param name="workingFolder">Working folder.</param>
    /// <param name="config">Configuration.</param>
    /// <param name="fileService">File service.</param>
    /// <param name="logger">Logger.</param>
    public InventoryAction(string workingFolder, AssembleConfiguration config, IFileService fileService, ILogger logger)
    {
        _workingFolder = workingFolder;
        _config = config;
        _fileService = fileService;
        _logger = logger;

        _fileInfoService = new(workingFolder, _fileService, _logger);

        // set full path of output folder
        _outputFolder = _fileService.GetFullPath(Path.Combine(_workingFolder, _config.DestinationFolder));
    }

    /// <summary>
    /// Gets the list of files. This is a result from the RunAsync() method.
    /// </summary>
    public List<FileData> Files { get; private set; } = [];

    /// <summary>
    /// Run the action.
    /// </summary>
    /// <returns>0 on success, 1 on warning, 2 on error.</returns>
    public Task<ReturnCode> RunAsync()
    {
        ReturnCode ret = ReturnCode.Normal;
        _logger.LogInformation($"\n*** INVENTORY STAGE.");

        try
        {
            ret = GetAllFiles();
            ret &= ValidateFiles();

            if (ret != ReturnCode.Error)
            {
                ret &= UpdateLinks();

                // log result of inventory (verbose)
                foreach (var file in Files)
                {
                    _logger.LogInformation($"{file.SourcePath}  \n\t==>  {file.DestinationPath}");
                    foreach (var link in file.Links)
                    {
                        _logger.LogInformation($"\t{link.OriginalUrl} => {link.DestinationRelativeUrl ?? link.DestinationFullUrl}");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogCritical($"Inventory error: {ex.Message}");
            ret = ReturnCode.Error;
        }

        _logger.LogInformation($"END OF INVENTORY STAGE. Result: {ret}");
        return Task.FromResult(ret);
    }

    private ReturnCode UpdateLinks()
    {
        ReturnCode ret = ReturnCode.Normal;

        foreach (var file in Files)
        {
            if (file.Links.Count > 0)
            {
                _logger.LogInformation($"Updating links for '{file.SourcePath}'");

                foreach (var link in file.Links)
                {
                    var dest = Files.SingleOrDefault(x => x.SourcePath.Equals(link.UrlFullPath, StringComparison.Ordinal));
                    if (dest != null)
                    {
                        // destination found. register and also (new) calculate relative path
                        link.DestinationFullUrl = dest.DestinationPath;
                        string dir = Path.GetDirectoryName(file.DestinationPath)!;
                        link.DestinationRelativeUrl = Path.GetRelativePath(dir, dest.DestinationPath).NormalizePath();
                        if (!string.IsNullOrEmpty(link.UrlTopic))
                        {
                            link.DestinationFullUrl += link.UrlTopic;
                            link.DestinationRelativeUrl += link.UrlTopic;
                        }
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(file.ContentSet!.ExternalFilePrefix))
                        {
                            // ERROR: no solution to fix this reference
                            _logger.LogCritical($"Error in a file reference. Link '{link.OriginalUrl}' in '{file.SourcePath}' cannot be resolved and no external file prefix was given.");
                            ret = ReturnCode.Error;
                        }
                        else
                        {
                            // we're calculating the link with the external file prefix, usualy a repo web link prefix.
                            string subpath = link.UrlFullPath.Substring(file.ContentSet!.SourceFolder.Length).TrimStart('/');
                            link.DestinationFullUrl = file.ContentSet!.ExternalFilePrefix.TrimEnd('/') + "/" + subpath;
                        }
                    }
                }
            }
        }

        return ret;
    }

    private ReturnCode ValidateFiles()
    {
        ReturnCode ret = ReturnCode.Normal;

        var duplicates = Files.GroupBy(x => x.DestinationPath).Where(g => g.Count() > 1);
        if (duplicates.Any())
        {
            _logger.LogCritical("ERROR: one or more files will be overwritten. Validate content definitions. Consider exclude paths.");
            foreach (var dup in duplicates)
            {
                _logger.LogCritical($"{dup.Key} used for:");
                foreach (var source in dup)
                {
                    _logger.LogCritical($"\t{source.SourcePath} (Content group '{source.ContentSet!.SourceFolder}')");
                }
            }

            ret = ReturnCode.Error;
        }

        return ret;
    }

    private ReturnCode GetAllFiles()
    {
        ReturnCode ret = ReturnCode.Normal;

        // loop through all content definitions
        foreach (var content in _config.Content)
        {
            // determine source and destination folders
            var sourceFolder = _fileService.GetFullPath(Path.Combine(_workingFolder, content.SourceFolder));
            var destFolder = _outputFolder!;
            if (!string.IsNullOrEmpty(content.DestinationFolder))
            {
                destFolder = _fileService.GetFullPath(Path.Combine(destFolder, content.DestinationFolder.Trim()));
            }

            _logger.LogInformation($"Processing content for '{sourceFolder}' => '{destFolder}'");

            // get all files and loop through them to add to the this.Files collection
            var files = _fileService.GetFiles(sourceFolder, content.Files, content.Exclude);
            foreach (var file in files)
            {
                _logger.LogInformation($"- '{file}'");
                FileData fileData = new FileData
                {
                    ContentSet = content,
                    SourcePath = file.NormalizePath(),
                };

                if (content.RawCopy != true && Path.GetExtension(file).Equals(".md", StringComparison.OrdinalIgnoreCase))
                {
                    // only for markdown, get the links
                    fileData.Links = _fileInfoService.GetLocalHyperlinks(sourceFolder, file);
                }

                // set destination path of the file
                string subpath = fileData.SourcePath.Substring(sourceFolder.Length).TrimStart('/');
                fileData.DestinationPath = _fileService.GetFullPath(Path.Combine(destFolder, subpath));

                // if replace patterns are defined, apply them to the destination path
                if (content.UrlReplacements != null)
                {
                    try
                    {
                        // apply all replacements
                        foreach (var replacement in content.UrlReplacements)
                        {
                            string r = replacement.Value ?? string.Empty;
                            fileData.DestinationPath = Regex.Replace(fileData.DestinationPath, replacement.Expression, r);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Regex error for file `{file}`: {ex.Message}. No replacement done.");
                        ret = ReturnCode.Warning;
                    }
                }

                Files.Add(fileData);
            }
        }

        return ret;
    }
}
