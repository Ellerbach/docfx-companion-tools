// <copyright file="FileData.cs" company="DocFx Companion Tools">
// Copyright (c) DocFx Companion Tools. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
using System.Diagnostics.CodeAnalysis;

namespace DocFxTocGenerator.FileService;

/// <summary>
/// File data record.
/// </summary>
[ExcludeFromCodeCoverage]
public record FileData : FolderFileBase
{
    /// <summary>
    /// Gets a value indicating whether this is a markdown file.
    /// </summary>
    public bool IsMarkdown => System.IO.Path.GetExtension(Name).Equals(".md", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Gets a value indicating whether this is a swagger file.
    /// </summary>
    public bool IsSwagger => Name.EndsWith("swagger.json", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Gets a value indicating whether this is a README file.
    /// </summary>
    public bool IsConfiguration => Name.StartsWith(".", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Gets a value indicating whether this is a README file.
    /// </summary>
    public bool IsReadme => Name.Equals("readme.md", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Gets a value indicating whether this is an INDEX file.
    /// </summary>
    public bool IsIndex => Name.Equals("index.md", StringComparison.OrdinalIgnoreCase);
}
