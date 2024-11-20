// <copyright file="FolderNamingStrategy.cs" company="DocFx Companion Tools">
// Copyright (c) DocFx Companion Tools. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
namespace DocAssembler.Configuration;

/// <summary>
/// Copy strategy.
/// </summary>
public enum FolderNamingStrategy
{
    /// <summary>
    /// Use name of the source folder.
    /// </summary>
    SourceFolder,

    /// <summary>
    /// Use name of the parent folder of the source.
    /// </summary>
    ParentFolder,
}
