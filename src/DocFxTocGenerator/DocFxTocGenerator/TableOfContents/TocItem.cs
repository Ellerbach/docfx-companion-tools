// <copyright file="TocItem.cs" company="DocFx Companion Tools">
// Copyright (c) DocFx Companion Tools. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
using DocFxTocGenerator.FileService;

namespace DocFxTocGenerator.TableOfContents;

/// <summary>
/// Table of contents item.
/// </summary>
public class TocItem
{
    /// <summary>
    /// Gets or sets the name of the item.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the depth of this item in the complete hierarchy.
    /// </summary>
    public int Depth { get; set; }

    /// <summary>
    /// Gets or sets the reference path of the item.
    /// </summary>
    public string Href { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the sequence of the item (for sorting).
    /// </summary>
    public int Sequence { get; set; } = int.MaxValue;

    /// <summary>
    /// Gets or sets the base folder or file for this item.
    /// </summary>
    public FolderFileBase? Base { get; set; }

    /// <summary>
    /// Gets a value indicating whether this is a folder item.
    /// </summary>
    public bool IsFolder => Base != null && Base is FolderData;

    /// <summary>
    /// Gets a value indicating whether this is a file item.
    /// </summary>
    public bool IsFile => Base != null && Base is FileData;

    /// <summary>
    /// Gets or sets child items.
    /// </summary>
    public List<TocItem> Items { get; set; } = new();
}
