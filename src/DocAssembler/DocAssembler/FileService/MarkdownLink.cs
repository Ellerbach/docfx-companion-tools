// <copyright file="MarkdownLink.cs" company="DocFx Companion Tools">
// Copyright (c) DocFx Companion Tools. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
namespace DocAssembler.FileService;

/// <summary>
/// Markdown link in document.
/// </summary>
public sealed record MarkdownLink
{
    /// <summary>
    /// Gets or sets the URL.
    /// </summary>
    public string Url { get; set; } = string.Empty;
}
