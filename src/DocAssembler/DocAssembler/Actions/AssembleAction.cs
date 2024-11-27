// <copyright file="AssembleAction.cs" company="DocFx Companion Tools">
// Copyright (c) DocFx Companion Tools. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
using System.Text;
using System.Text.RegularExpressions;
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
                if (file.IsMarkdown && (updates.Any() || file.ContentSet?.ContentReplacements is not null))
                {
                    var markdown = _fileService.ReadAllText(file.SourcePath);
                    StringBuilder sb = new StringBuilder();
                    int pos = 0;
                    foreach (var update in updates)
                    {
                        // first append text so far from markdown
                        sb.Append(markdown.AsSpan(pos, update.UrlSpanStart - pos));

                        // append new link
                        sb.Append(update.DestinationRelativeUrl ?? update.DestinationFullUrl);

                        // set new starting position
                        pos = update.UrlSpanEnd + 1;
                    }

                    // add final part of markdown
                    sb.Append(markdown.AsSpan(pos));
                    string output = sb.ToString();

                    // if replacement patterns are defined, apply them to the content
                    int replacements = 0;
                    if (file.ContentSet?.ContentReplacements is not null)
                    {
                        try
                        {
                            // apply all replacements
                            foreach (var replacement in file.ContentSet.ContentReplacements)
                            {
                                string r = replacement.Value ?? string.Empty;
                                output = Regex.Replace(output, replacement.Expression, r);
                                replacements++;
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError($"Regex error for source `{file.SourcePath}`: {ex.Message}. No replacement done.");
                            ret = ReturnCode.Warning;
                        }
                    }

                    Directory.CreateDirectory(Path.GetDirectoryName(file.DestinationPath)!);
                    _fileService.WriteAllText(file.DestinationPath, output);
                    _logger.LogInformation($"Copied '{file.SourcePath}' to '{file.DestinationPath}' with {updates.Count()} URL replacements and {replacements} content replacements.");
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
