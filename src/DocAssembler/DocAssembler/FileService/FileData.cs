// <copyright file="FileData.cs" company="DocFx Companion Tools">
// Copyright (c) DocFx Companion Tools. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
namespace DocAssembler.FileService;

/// <summary>
/// File data.
/// </summary>
public sealed record FileData
{
    /// <summary>
    /// Gets or sets the original path of the file.
    /// </summary>
    public string OriginalPath { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the output path of the file.
    /// </summary>
    public string OutputPath { get; set; } = string.Empty;
}
