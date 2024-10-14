// <copyright file="IndexGenerationStrategy.cs" company="DocFx Companion Tools">
// Copyright (c) DocFx Companion Tools. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
namespace DocFxTocGenerator.Index;

/// <summary>
/// Enumeration of the type of generation for index.
/// </summary>
public enum IndexGenerationStrategy
{
    /// <summary>
    /// Do not generate an index.
    /// </summary>
    Never,

    /// <summary>
    /// Generate an index for all empty folders only.
    /// </summary>
    EmptyFolders,

    /// <summary>
    /// Generate an index for all folders that don't have an index file.
    /// </summary>
    NotExists,

    /// <summary>
    /// Generate an index for all folders that don't have an index file, except when it contains only 1 file.
    /// </summary>
    NotExistMultipleFiles,
}
