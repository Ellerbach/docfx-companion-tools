// <copyright file="FileService.cs" company="DocFx Companion Tools">
// Copyright (c) DocFx Companion Tools. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.FileSystemGlobbing;

namespace DocAssembler.FileService;

/// <summary>
/// File service implementation working with <see cref="File"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class FileService : IFileService
{
    /// <inheritdoc />
    public string GetFullPath(string path)
    {
        return Path.GetFullPath(path).NormalizePath();
    }

    /// <inheritdoc />
    public bool ExistsFileOrDirectory(string path)
    {
        return File.Exists(path) || Directory.Exists(path);
    }

    /// <inheritdoc />
    public IEnumerable<string> GetFiles(string root, List<string> includes, List<string>? excludes)
    {
        string fullRoot = Path.GetFullPath(root);
        Matcher matcher = new();
        foreach (string folderName in includes)
        {
            matcher.AddInclude(folderName);
        }

        if (excludes != null)
        {
            foreach (string folderName in excludes)
            {
                matcher.AddExclude(folderName);
            }
        }

        // make sure we normalize the directory separator
        return matcher.GetResultsInFullPath(fullRoot)
            .Select(x => x.NormalizePath())
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

    /// <inheritdoc/>
    public void Copy(string source, string destination)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(destination)!);
        File.Copy(source, destination);
    }

    /// <inheritdoc/>
    public void DeleteFolder(string path)
    {
        if (Directory.Exists(path))
        {
            Directory.Delete(path);
        }
    }
}
