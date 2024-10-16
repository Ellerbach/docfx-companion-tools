// <copyright file="FileService.cs" company="DocFx Companion Tools">
// Copyright (c) DocFx Companion Tools. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
using Microsoft.Extensions.FileSystemGlobbing;

namespace DocFxTocGenerator.FileService;

/// <summary>
/// File service implementation working with <see cref="File"/> class.
/// </summary>
public class FileService : IFileService
{
    /// <inheritdoc />
    public string GetFullPath(string path)
    {
        return Path.GetFullPath(path);
    }

    /// <inheritdoc />
    public bool ExistsFileOrDirectory(string path)
    {
        return File.Exists(path) || Directory.Exists(path);
    }

    /// <inheritdoc />
    public IEnumerable<string> GetFiles(string root, List<string> includes, List<string> excludes)
    {
        string fullRoot = Path.GetFullPath(root);
        Matcher matcher = new();
        foreach (string folderName in includes)
        {
            matcher.AddInclude(folderName);
        }

        foreach (string folderName in excludes)
        {
            matcher.AddExclude(folderName);
        }

        // make sure we normalize the directory separator
        return matcher.GetResultsInFullPath(fullRoot)
            .Select(x => x.Replace("\\", "/"))
            .ToList();
    }

    /// <inheritdoc />
    public IEnumerable<string> GetDirectories(string folder)
    {
        return Directory.GetDirectories(folder);
    }

    /// <inheritdoc/>>
    public string ReadAllText(string path)
    {
        return File.ReadAllText(path);
    }

    /// <inheritdoc/>
    public string[] ReadAllLines(string path)
    {
        return File.ReadAllLines(path);
    }

    /// <inheritdoc/>
    public void WriteAllText(string path, string content)
    {
        File.WriteAllText(path, content);
    }

    /// <inheritdoc/>
    public Stream OpenRead(string path)
    {
        return File.OpenRead(path);
    }
}
