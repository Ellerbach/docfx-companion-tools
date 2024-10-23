// <copyright file="TocFolderReferenceStrategy.cs" company="DocFx Companion Tools">
// Copyright (c) DocFx Companion Tools. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
namespace DocFxTocGenerator.TableOfContents;

/// <summary>
/// Table of contents folder reference strategy.
/// </summary>
public enum TocFolderReferenceStrategy
{
    /// <summary>
    /// Folders don't have a reference to anything.
    /// </summary>
    None,

    /// <summary>
    /// Folders can have a reference to an index file, otherwise no reference.
    /// </summary>
    Index,

    /// <summary>
    ///  Folders can have a reference to an index file or readme, otherwise no reference.
    /// </summary>
    IndexReadme,

    /// <summary>
    ///  Folders can have a reference to an index file or readme or the first file in the folder.
    ///  No reference if the folder is empty.
    /// </summary>
    First,
}
