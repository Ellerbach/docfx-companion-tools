// <copyright file="AssembleConfiguration.cs" company="DocFx Companion Tools">
// Copyright (c) DocFx Companion Tools. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
using System.Text.Json.Serialization;

namespace DocAssembler.Configuration;

/// <summary>
/// Assemble configuration.
/// </summary>
public sealed record AssembleConfiguration
{
    /// <summary>
    /// Gets or sets the destination folder.
    /// </summary>
    [JsonPropertyName("dest")]
    public string DestinationFolder { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the global URL replacements. Can be overruled by <see cref="Content"/> settings.
    /// </summary>
    public List<Replacement>? UrlReplacements { get; set; }

    /// <summary>
    /// Gets or sets the global content replacements. Can be overruled by <see cref="Content"/> settings.
    /// </summary>
    public List<Replacement>? ContentReplacements { get; set; }

    /// <summary>
    /// Gets or sets the prefix for external files like source files.
    /// This is for all references to files that are not part of the
    /// selected files (mostly markdown and assets).
    /// An example use is to prefix the URL with the url of the github repo.
    /// This is the global setting, that can be overruled by <see cref="Content"/> settings.
    /// </summary>
    public string? ExternalFilePrefix { get; set; }

    /// <summary>
    /// Gets or sets the content to process.
    /// </summary>
    public List<Content> Content { get; set; } = new();
}
