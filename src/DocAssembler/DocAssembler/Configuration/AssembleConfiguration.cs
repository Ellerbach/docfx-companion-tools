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
    /// Gets or sets the output folder.
    /// </summary>
    [JsonPropertyName("path")]
    public string OutputFolder { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the content to process.
    /// </summary>
    public List<Content> Content { get; set; } = new();
}
