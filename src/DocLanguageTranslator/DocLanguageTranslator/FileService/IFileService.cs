// Licensed to DocFX Companion Tools and contributors under one or more agreements.
// DocFX Companion Tools and contributors licenses this file to you under the MIT license.

namespace DocLanguageTranslator.FileService;

/// <summary>
/// Provides an abstraction for file system operations, enabling testability and flexibility when working with files and directories.
/// </summary>
internal interface IFileService
{
    /// <summary>
    /// Determines whether the specified directory exists.
    /// </summary>
    /// <param name="path">The path to the directory to check.</param>
    /// <returns>True if the directory exists; otherwise, false.</returns>
    bool DirectoryExists(string path);

    /// <summary>
    /// Checks if a file exists at the specified path.
    /// </summary>
    /// <param name="path">The path to the file to check.</param>
    /// <returns>
    ///   <c>true</c> if the file exists at the specified path; otherwise, <c>false</c>.
    /// </returns>
    bool FileExists(string path);

    /// <summary>
    /// Returns the names of files in the specified directory that match the specified search pattern and search option.
    /// </summary>
    /// <param name="path">The directory path to search.</param>
    /// <param name="searchPattern">The search string to match against file names (e.g., "*.txt").</param>
    /// <param name="searchOption">Specifies whether to search the current directory only or include all subdirectories.</param>
    /// <returns>An array of file paths that match the specified criteria.</returns>
    string[] GetFiles(string path, string searchPattern, SearchOption searchOption);

    /// <summary>
    /// Returns the names of subdirectories in the specified directory.
    /// </summary>
    /// <param name="path">The path to the directory to search.</param>
    /// <returns>An array of directory paths.</returns>
    string[] GetDirectories(string path);

    /// <summary>
    /// Reads all text from the specified file.
    /// </summary>
    /// <param name="filePath">The path to the file to read.</param>
    /// <returns>The contents of the file as a string.</returns>
    string ReadAllText(string filePath);

    /// <summary>
    /// Writes the specified string to a file, overwriting any existing content.
    /// </summary>
    /// <param name="filePath">The path to the file to write to.</param>
    /// <param name="content">The content to write to the file.</param>
    void WriteAllText(string filePath, string content);

    /// <summary>
    /// Creates a new directory at the specified path.
    /// </summary>
    /// <param name="path">The path of the directory to create.</param>
    void CreateDirectory(string path);
}
