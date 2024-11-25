// <copyright file="Content.cs" company="DocFx Companion Tools">
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
    [JsonPropertyName("dest")]
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
    public List<string>? Exclude { get; set; }

    /// <summary>
    /// Gets or sets the pattern to find in references to be replaced. This is a regex expression.
    /// Works with the the <see cref="ReplaceValue"/> to replace what was found.
    /// Example: "\/docs\/".
    /// </summary>
    public string? ReplacePattern { get; set; }

    /// <summary>
    /// Gets or sets the value to replace what was found with <see cref="ReplacePattern"/>.
    /// </summary>
    public string? ReplaceValue { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether we need to do just a raw copy.
    /// </summary>
    public bool? RawCopy { get; set; }

    /// <summary>
    /// Gets or sets the prefix for external files like source files.
    /// This is for all references to files that are not part of the
    /// selected files (mostly markdown and assets).
    /// An example use is to prefix the URL with the url of the github repo.
    /// </summary>
    public string? ExternalFilePrefix { get; set; }
}
