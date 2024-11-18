// <copyright file="AssembleAction.cs" company="DocFx Companion Tools">
// Copyright (c) DocFx Companion Tools. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
using DocAssembler.FileService;
using Microsoft.Extensions.Logging;

namespace DocAssembler.Actions;

/// <summary>
/// Assemble documentation in the output folder. The tool will also fix links following configuration.
/// </summary>
public class AssembleAction
{
    private readonly string _configFile;
    private readonly string? _outFolder;

    private readonly IFileService? _fileService;
    private readonly ILogger _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="AssembleAction"/> class.
    /// </summary>
    /// <param name="configFile">Configuration file.</param>
    /// <param name="outFolder">Output folder.</param>
    /// <param name="fileService">File service.</param>
    /// <param name="logger">Logger.</param>
    public AssembleAction(
        string configFile,
        string? outFolder,
        IFileService fileService,
        ILogger logger)
    {
        _configFile = configFile;
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
