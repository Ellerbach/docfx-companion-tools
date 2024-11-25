// <copyright file="FileData.cs" company="DocFx Companion Tools">
// Copyright (c) DocFx Companion Tools. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
using DocAssembler.Configuration;

namespace DocAssembler.FileService;

/// <summary>
/// File data.
/// </summary>
public sealed record FileData
{
    /// <summary>
    /// Gets or sets the source full path of the file.
    /// </summary>
    public string SourcePath { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the destination full path of the file.
    /// </summary>
    public string DestinationPath { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the content set the file belongs to.
    /// </summary>
    public Content? ContentSet { get; set; }

    /// <summary>
    /// Gets or sets all links in the document we might need to work on.
    /// </summary>
    public List<Hyperlink> Links { get; set; } = [];
}
