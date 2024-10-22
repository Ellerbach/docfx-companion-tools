// <copyright file="FolderFileBase.cs" company="DocFx Companion Tools">
// Copyright (c) DocFx Companion Tools. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
using System.Diagnostics.CodeAnalysis;

namespace DocFxTocGenerator.FileService;

/// <summary>
/// Base record for folders and files.
/// </summary>
[ExcludeFromCodeCoverage]
public record FolderFileBase
{
    /// <summary>
    /// Gets or sets the name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the full path.
    /// </summary>
    public string Path { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the display name of the item.
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the sequence value of this item.
    /// </summary>
    public int Sequence { get; set; } = int.MaxValue;

    /// <summary>
    /// Gets or sets the parent folder.
    /// </summary>
    public FolderData? Parent { get; set; }

    /// <summary>
    /// Gets the relative path to the item.
    /// </summary>
    public string RelativePath
    {
        get
        {
            return Parent == null ? string.Empty : System.IO.Path.Combine(Parent.RelativePath, Name);
        }
    }

    /// <summary>
    /// Gets the root full path of the hierarchy.
    /// </summary>
    public string RootFullPath
    {
        get
        {
            return Parent == null ? Path : Parent.RootFullPath;
        }
    }
}
