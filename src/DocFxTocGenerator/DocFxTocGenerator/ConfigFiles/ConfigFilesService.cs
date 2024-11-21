// <copyright file="ConfigFilesService.cs" company="DocFx Companion Tools">
// Copyright (c) DocFx Companion Tools. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
using DocFxTocGenerator.FileService;
using Microsoft.Extensions.Logging;

namespace DocFxTocGenerator.ConfigFiles;

/// <summary>
/// Service to read configuration files in directories.
/// </summary>
public class ConfigFilesService
{
    private readonly bool _camelCasing;
    private readonly IFileService _fileService;
    private readonly ILogger _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigFilesService"/> class.
    /// </summary>
    /// <param name="camelCasing">Use camel casing for titles.</param>
    /// <param name="fileService">File service.</param>
    /// <param name="logger">Logger.</param>
    public ConfigFilesService(bool camelCasing, IFileService fileService, ILogger logger)
    {
        _camelCasing = camelCasing;
        _fileService = fileService;
        _logger = logger;
    }

    /// <summary>
    /// Gets the default order list containing "readme" and "index".
    /// </summary>
    public static List<string> DefaultOrderList => new() { "index", "readme" };

    /// <summary>
    /// If the .order file exists in the directory, read it and
    /// return the list of file names in the required order.
    /// </summary>
    /// <param name="dirPath">Directory to check.</param>
    /// <returns>List of file names in order. Just containing "readme" and "index" if file doesn't exist.</returns>
    public List<string> GetOrderList(string dirPath)
    {
        string path = Path.Combine(dirPath, ".order");
        List<string> orderList = new();
        if (_fileService.ExistsFileOrDirectory(path))
        {
            _logger.LogInformation($"Read existing order file {path}");
            orderList = _fileService.ReadAllLines(path).ToList();
        }

        int insertIndex = 0;
        foreach (string file in DefaultOrderList)
        {
            if (!orderList.Contains(file, StringComparer.OrdinalIgnoreCase))
            {
                // we use insertIndex to keep the sequence from the DefaultOrderList.
                orderList.Insert(insertIndex++, file);
            }
        }

        return orderList;
    }

    /// <summary>
    /// If the .ignore file exists in the directory, read it and
    /// return the list of directory names to ignore in this directory.
    /// </summary>
    /// <param name="dirPath">Directory to check.</param>
    /// <returns>List of directory names to ignore. Empty if file doesn't exist.</returns>
    public List<string> GetIgnoreList(string dirPath)
    {
        string path = Path.Combine(dirPath, ".ignore");
        if (_fileService.ExistsFileOrDirectory(path))
        {
            _logger.LogInformation($"Read existing order file {path}");
            return _fileService.ReadAllLines(path).ToList();
        }

        return new();
    }

    /// <summary>
    /// If the .overrides file exists in the directory, read it and
    /// return the dictionary with Filename as key and the override
    /// title in the value.
    /// </summary>
    /// <param name="dirPath">Directory to check.</param>
    /// <returns>List of filename,title overrides. Empty if file doesn't exist.</returns>
    public Dictionary<string, string> GetOverrideList(string dirPath)
    {
        string path = Path.Combine(dirPath, ".override");
        if (_fileService.ExistsFileOrDirectory(path))
        {
            _logger.LogInformation($"Read existing order file {path}");
            var overrides = new Dictionary<string, string>();
            foreach (var over in _fileService.ReadAllLines(path))
            {
                var parts = over.Split(';');
                if (parts?.Length == 2)
                {
                    overrides.TryAdd(parts[0], parts[1]);
                }
            }

            return overrides;
        }

        return new();
    }
}
