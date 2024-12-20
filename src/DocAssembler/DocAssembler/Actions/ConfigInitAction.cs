﻿// <copyright file="ConfigInitAction.cs" company="DocFx Companion Tools">
// Copyright (c) DocFx Companion Tools. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
using DocAssembler.Configuration;
using DocAssembler.FileService;
using DocAssembler.Utils;
using Microsoft.Extensions.Logging;

namespace DocAssembler.Actions;

/// <summary>
/// Initialize and save an initial configuration file if it doesn't exist yet.
/// </summary>
public class ConfigInitAction
{
    private const string ConfigFileName = ".docassembler.json";

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

        try
        {
            string path = Path.Combine(_outFolder, ConfigFileName);
            if (_fileService!.ExistsFileOrDirectory(path))
            {
                _logger.LogError($"*** ERROR: '{path}' already exists. We don't overwrite.");

                // indicate we're done with an error
                return Task.FromResult(ReturnCode.Error);
            }

            var config = new AssembleConfiguration
            {
                DestinationFolder = "out",
                ExternalFilePrefix = "https://github.com/example/blob/main/",
                Content =
                [
                    new Content
                    {
                        SourceFolder = ".docfx",
                        Files = { "**" },
                        RawCopy = true,
                    },
                    new Content
                    {
                        SourceFolder = "docs",
                        Files = { "**" },
                    },
                    new Content
                    {
                        SourceFolder = "backend",
                        DestinationFolder = "services",
                        Files = { "**/docs/**" },
                        UrlReplacements = [
                                new Replacement
                                {
                                    Expression = "/[Dd]ocs/",
                                    Value = "/",
                                }
                            ],
                    },
                ],
            };

            _fileService.WriteAllText(path, SerializationUtil.Serialize(config));
            _logger.LogInformation($"Initial configuration saved in '{path}'");
        }
        catch (Exception ex)
        {
            _logger.LogCritical($"Saving initial configuration error: {ex.Message}.");
            ret = ReturnCode.Error;
        }

        return Task.FromResult(ret);
    }
}
