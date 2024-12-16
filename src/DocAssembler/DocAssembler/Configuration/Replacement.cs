// <copyright file="Replacement.cs" company="DocFx Companion Tools">
// Copyright (c) DocFx Companion Tools. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
namespace DocAssembler.Configuration;

/// <summary>
/// Replacement definition.
/// </summary>
public sealed record Replacement
{
    /// <summary>
    /// Gets or sets the regex expression for the replacement.
    /// </summary>
    public string Expression { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the replacement value.
    /// </summary>
    public string? Value { get; set; }
}
