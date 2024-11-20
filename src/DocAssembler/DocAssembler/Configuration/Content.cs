﻿// <copyright file="Content.cs" company="DocFx Companion Tools">
// Copyright (c) DocFx Companion Tools. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
using System.Text.Json.Serialization;

namespace DocAssembler.Configuration;

/// <summary>
/// Content definition using globbing patterns.
/// </summary>
public sealed record Content
{
    /// <summary>
    /// Gets or sets the source folder.
    /// </summary>
    [JsonPropertyName("src")]
    public string SourceFolder { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the optional destination folder.
    /// </summary>
    [JsonPropertyName("path")]
    public string? DestinationFolder { get; set; }

    /// <summary>
    /// Gets or sets the folders and files to include.
    /// This list supports the file glob pattern.
    /// </summary>
    public List<string> Files { get; set; } = new();

    /// <summary>
    /// Gets or sets the folders and files to exclude.
    /// This list supports the file glob pattern.
    /// </summary>
    public List<string> Exclude { get; set; } = new();

    /// <summary>
    /// Gets or sets naming strategy of the target folder.
    /// </summary>
    public FolderNamingStrategy Naming { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether we need to fix markdown links.
    /// </summary>
    public bool FixMarkdownLinks { get; set; } = true;

    /// <summary>
    /// Gets or sets the prefix for external files like source files.
    /// This is for all references to files that are not part of the
    /// documentation hierarchy. Use this to prefix Gitlab URL's.
    /// </summary>
    public string ExternalFilePrefix { get; set; } = string.Empty;
}
