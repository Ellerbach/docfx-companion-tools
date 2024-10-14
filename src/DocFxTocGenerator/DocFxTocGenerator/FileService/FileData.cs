// <copyright file="FileData.cs" company="DocFx Companion Tools">
// Copyright (c) DocFx Companion Tools. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
namespace DocFxTocGenerator.FileService;

/// <summary>
/// File data record.
/// </summary>
public record FileData : FolderFileBase
{
    /// <summary>
    /// Gets a value indicating whether this is a README file.
    /// </summary>
    public bool IsReadme => string.Equals(Name, "readme.md", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Gets a value indicating whether this is an INDEX file.
    /// </summary>
    public bool IsIndex => string.Equals(Name, "index.md", StringComparison.OrdinalIgnoreCase);
}
