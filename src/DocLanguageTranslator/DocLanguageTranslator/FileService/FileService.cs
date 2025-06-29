// Licensed to DocFX Companion Tools and contributors under one or more agreements.
// DocFX Companion Tools and contributors licenses this file to you under the MIT license.

namespace DocLanguageTranslator.FileService;

/// <summary>
/// Provides file operations implementation.
/// </summary>
internal class FileService : IFileService
{
    /// <inheritdoc/>
    public bool DirectoryExists(string path) => Directory.Exists(path);

    /// <inheritdoc/>
    public bool FileExists(string path) => File.Exists(path);

    /// <inheritdoc/>
    public string[] GetFiles(string path, string searchPattern, SearchOption searchOption)
        => Directory.GetFiles(path, searchPattern, searchOption);

    /// <inheritdoc/>
    public string[] GetDirectories(string path)
        => Directory.GetDirectories(path);

    /// <inheritdoc/>
    public string ReadAllText(string filePath)
        => File.ReadAllText(filePath);

    /// <inheritdoc/>
    public void WriteAllText(string filePath, string content)
        => File.WriteAllText(filePath, content);

    /// <inheritdoc/>
    public void CreateDirectory(string path)
        => Directory.CreateDirectory(path);
}
