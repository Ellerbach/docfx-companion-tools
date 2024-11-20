// <copyright file="InventoryAction.cs" company="DocFx Companion Tools">
// Copyright (c) DocFx Companion Tools. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
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
    private readonly string _configFile;
    private readonly string? _outputFolder;
    private readonly FileInfoService _fileInfoService;
    private readonly IFileService _fileService;
    private readonly ILogger _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="InventoryAction"/> class.
    /// </summary>
    /// <param name="workingFolder">Working folder.</param>
    /// <param name="configFile">Configuration file path.</param>
    /// <param name="outputFolder">Output folder override.</param>
    /// <param name="fileService">File service.</param>
    /// <param name="logger">Logger.</param>
    public InventoryAction(string workingFolder, string configFile, string? outputFolder, IFileService fileService, ILogger logger)
    {
        _workingFolder = workingFolder;
        _configFile = configFile;
        _outputFolder = outputFolder;
        _fileService = fileService;
        _logger = logger;

        _fileInfoService = new(workingFolder, _fileService, _logger);
    }

    /// <summary>
    /// Run the action.
    /// </summary>
    /// <returns>0 on success, 1 on warning, 2 on error.</returns>
    public Task<ReturnCode> RunAsync()
    {
        ReturnCode ret = ReturnCode.Normal;

        try
        {
            AssembleConfiguration config = ReadConfigurationAsync(_configFile);
            if (!string.IsNullOrWhiteSpace(_outputFolder))
            {
                // overwrite output folder with given value
                config.OutputFolder = _outputFolder;
            }

            foreach (var content in config.Content)
            {
                var source = Path.Combine(_workingFolder, content.SourceFolder);
                var files = _fileService.GetFiles(source, content.Files, content.Exclude);
                foreach (var file in files)
                {
                    _fileInfoService.GetExternalHyperlinks(source, file);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogCritical($"Reading configuration error: {ex.Message}");
            ret = ReturnCode.Error;
        }

        return Task.FromResult(ret);
    }

    private AssembleConfiguration ReadConfigurationAsync(string configFile)
    {
        if (!_fileService.ExistsFileOrDirectory(configFile))
        {
            throw new ActionException($"Configuration file '{configFile}' doesn't exist.");
        }

        string json = _fileService.ReadAllText(configFile);
        return SerializationUtil.Deserialize<AssembleConfiguration>(json);
    }
}
