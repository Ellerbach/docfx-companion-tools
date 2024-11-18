// <copyright file="ConfigInitAction.cs" company="DocFx Companion Tools">
// Copyright (c) DocFx Companion Tools. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
using DocAssembler.FileService;
using Microsoft.Extensions.Logging;

namespace DocAssembler.Actions;

/// <summary>
/// Initialize and save an initial configuration file if it doesn't exist yet.
/// </summary>
public class ConfigInitAction
{
    private readonly string _outFolder;

    private readonly IFileService? _fileService;
    private readonly ILogger _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigInitAction"/> class.
    /// </summary>
    /// <param name="outFolder">Output folder.</param>
    /// <param name="fileService">File service.</param>
    /// <param name="logger">Logger.</param>
    public ConfigInitAction(
        string outFolder,
        IFileService fileService,
        ILogger logger)
    {
        _outFolder = outFolder;

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
        _logger.LogInformation($"\n*** INVENTORY STAGE.");

        try
        {
            ret = ReturnCode.Warning;
        }
        catch (Exception ex)
        {
            _logger.LogCritical($"Inventory error: {ex.Message}.");
            ret = ReturnCode.Error;
        }

        _logger.LogInformation($"END OF INVENTORY STAGE. Result: {ret}");
        return Task.FromResult(ret);
    }
}
