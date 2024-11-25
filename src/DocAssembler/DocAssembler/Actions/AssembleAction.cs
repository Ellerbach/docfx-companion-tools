﻿// <copyright file="AssembleAction.cs" company="DocFx Companion Tools">
// Copyright (c) DocFx Companion Tools. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
using System.Diagnostics;
using System.Text;
using DocAssembler.FileService;
using Microsoft.Extensions.Logging;

namespace DocAssembler.Actions;

/// <summary>
/// Assemble documentation in the output folder. The tool will also fix links following configuration.
/// </summary>
public class AssembleAction
{
    private readonly List<FileData> _files;
    private readonly IFileService _fileService;
    private readonly ILogger _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="AssembleAction"/> class.
    /// </summary>
    /// <param name="files">List of files to process.</param>
    /// <param name="fileService">File service.</param>
    /// <param name="logger">Logger.</param>
    public AssembleAction(
        List<FileData> files,
        IFileService fileService,
        ILogger logger)
    {
        _files = files;
        _fileService = fileService;
        _logger = logger;
    }

    /// <summary>
    /// Run the action.
    /// </summary>
    /// <returns>0 on success, 1 on warning, 2 on error.</returns>
    public Task<ReturnCode> RunAsync()
    {
        ReturnCode ret = ReturnCode.Normal;
        _logger.LogInformation($"\n*** ASSEMBLE STAGE.");

        try
        {
            foreach (var file in _files)
            {
                // get all links that need to be changed
                var updates = file.Links
                    .Where(x => !x.OriginalUrl.Equals(x.DestinationRelativeUrl ?? x.DestinationFullUrl, StringComparison.Ordinal))
                    .OrderBy(x => x.UrlSpanStart);
                if (updates.Any())
                {
                    var markdown = _fileService.ReadAllText(file.SourcePath);
                    StringBuilder sb = new StringBuilder();
                    int pos = 0;
                    foreach (var update in updates)
                    {
                        // first append text so far from markdown
                        Console.WriteLine($"pos={pos} len={update.UrlSpanStart - pos} md={markdown.Length}");
                        sb.Append(markdown.AsSpan(pos, update.UrlSpanStart - pos));

                        // append new link
                        sb.Append(update.DestinationRelativeUrl ?? update.DestinationFullUrl);

                        // set new starting position
                        pos = update.UrlSpanEnd + 1;
                    }

                    // add final part of markdown
                    sb.Append(markdown.AsSpan(pos));

                    Directory.CreateDirectory(Path.GetDirectoryName(file.DestinationPath)!);
                    _fileService.WriteAllText(file.DestinationPath, sb.ToString());
                    _logger.LogInformation($"Copied '{file.SourcePath}' to '{file.DestinationPath}'. Replace {updates.Count()} links.");
                }
                else
                {
                    _fileService.Copy(file.SourcePath, file.DestinationPath);
                    _logger.LogInformation($"Copied '{file.SourcePath}' to '{file.DestinationPath}'.");
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogCritical($"Assembly error: {ex.Message}.");
            ret = ReturnCode.Error;
        }

        _logger.LogInformation($"END OF ASSEMBLE STAGE. Result: {ret}");
        return Task.FromResult(ret);
    }
}
