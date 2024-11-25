// <copyright file="IFileService.cs" company="DocFx Companion Tools">
// Copyright (c) DocFx Companion Tools. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
namespace DocAssembler.FileService;

/// <summary>
/// File service interface. This is to hide file system access behind an interface.
/// This allows for the implementation of a mock for unit testing.
/// </summary>
public interface IFileService
{
    /// <summary>
    /// Get the full path of the given path.
    /// </summary>
    /// <param name="path">Path of file or folder.</param>
    /// <returns>The full path of the file or folder.</returns>
    string GetFullPath(string path);

    /// <summary>
    /// Check if the given path exists as file or directory.
    /// </summary>
    /// <param name="path">Path to check.</param>
    /// <returns>A value indicating whether the path exists.</returns>
    bool ExistsFileOrDirectory(string path);

    /// <summary>
    /// Get files with the Glob File Pattern.
    /// </summary>
    /// <param name="root">Root path.</param>
    /// <param name="includes">Include patterns.</param>
    /// <param name="excludes">Exclude patterns.</param>
    /// <returns>List of files.</returns>
    IEnumerable<string> GetFiles(string root, List<string> includes, List<string>? excludes);

    /// <summary>
    /// Get directories in the given path.
    /// </summary>
    /// <param name="folder">Folder path.</param>
    /// <returns>List of folders.</returns>
    IEnumerable<string> GetDirectories(string folder);

    /// <summary>
    /// Read the file as text string.
    /// </summary>
    /// <param name="path">Path of the file.</param>
    /// <returns>Contents of the file or empty if doesn't exist.</returns>
    string ReadAllText(string path);

    /// <summary>
    /// Read the file as array of strings split on newlines.
    /// </summary>
    /// <param name="path">Path of the file.</param>
    /// <returns>All lines of text or empty if doesn't exist.</returns>
    string[] ReadAllLines(string path);

    /// <summary>
    /// Write content to given path.
    /// </summary>
    /// <param name="path">Path of the file.</param>
    /// <param name="content">Content to write to the file.</param>
    void WriteAllText(string path, string content);

    /// <summary>
    /// Get a stream for the given path to read.
    /// </summary>
    /// <param name="path">Path of the file.</param>
    /// <returns>A <see cref="Stream"/>.</returns>
    Stream OpenRead(string path);

    /// <summary>
    /// Copy the given file to the destination.
    /// </summary>
    /// <param name="source">Source file path.</param>
    /// <param name="destination">Destination file path.</param>
    void Copy(string source, string destination);
}
