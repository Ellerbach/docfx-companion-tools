// <copyright file="TocOrderStrategy.cs" company="DocFx Companion Tools">
// Copyright (c) DocFx Companion Tools. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
namespace DocFxTocGenerator.TableOfContents;

/// <summary>
/// Table of contents order strategy.
/// </summary>
public enum TocOrderStrategy
{
    /// <summary>
    /// Order folders and files in one list.
    /// </summary>
    All,

    /// <summary>
    /// Order folders first, then order files.
    /// </summary>
    FoldersFirst,

    /// <summary>
    ///  Order files first, then order folders.
    /// </summary>
    FilesFirst,
}
