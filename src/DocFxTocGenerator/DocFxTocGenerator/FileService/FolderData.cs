// <copyright file="FolderData.cs" company="DocFx Companion Tools">
// Copyright (c) DocFx Companion Tools. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
namespace DocFxTocGenerator.FileService;

/// <summary>
/// Folder data record.
/// </summary>
public record FolderData : FolderFileBase
{
    /// <summary>
    /// Gets or sets the folders in this folder.
    /// </summary>
    public List<FolderData> Folders { get; set; } = new();

    /// <summary>
    /// Gets or sets the files in this folder.
    /// </summary>
    public List<FileData> Files { get; set; } = new();

    /// <summary>
    /// Gets or sets the order list.
    /// </summary>
    public List<string> OrderList { get; set; } = new();

    /// <summary>
    /// Gets or sets the ignore list.
    /// </summary>
    public List<string> IgnoreList { get; set; } = new();

    /// <summary>
    /// Gets or sets the overrides list.
    /// </summary>
    public Dictionary<string, string> OverrideList { get; set; } = new();

    /// <summary>
    /// Gets the number of folders in this folder.
    /// </summary>
    public int FolderCount => Folders.Count;

    /// <summary>
    /// Gets the number of files in this folder.
    /// </summary>
    public int FileCount => Files.Count;

    /// <summary>
    /// Gets a value indicating whether there is a readme in this folder.
    /// </summary>
    public bool HasReadme => Files.Any(x => x.IsReadme);

    /// <summary>
    /// Gets a value indicating whether tjere is an index in this folder.
    /// </summary>
    public bool HasIndex => Files.Any(x => x.IsIndex);

    /// <summary>
    /// Gets the readme in this folder, or null if it doesn't exist.
    /// </summary>
    public FileData? Readme => Files.FirstOrDefault(x => x.IsReadme);

    /// <summary>
    /// Gets the index file in this folder, or null if it doesn't exist.
    /// </summary>
    public FileData? Index => Files.FirstOrDefault(x => x.IsIndex);

    /// <summary>
    /// Find the <see cref="FolderData"/> object for the given relative path.
    /// </summary>
    /// <param name="path">Relative path to find in the hierarchy.</param>
    /// <returns>Found <see cref="FolderData"/> object. Null when not found.</returns>
    public FolderData? Find(string path)
    {
        string search = path.NormalizePath();
        if ((System.IO.Path.IsPathRooted(search) && search.Equals(Path, StringComparison.OrdinalIgnoreCase)) ||
            (!System.IO.Path.IsPathRooted(search) && search.Equals(RelativePath, StringComparison.OrdinalIgnoreCase)))
        {
            return this;
        }

        string[] subPaths = search.Split('/');

        FolderData? current = this;
        int i = 0;
        while (i < subPaths.Length)
        {
            if (!string.IsNullOrEmpty(subPaths[i]))
            {
                current = current!.Folders.FirstOrDefault(x => x.Name.Equals(subPaths[i], StringComparison.OrdinalIgnoreCase));
                if (current == null)
                {
                    // sub path not found. Stop searching.
                    return null;
                }
            }

            i++;
        }

        return current;
    }
}
