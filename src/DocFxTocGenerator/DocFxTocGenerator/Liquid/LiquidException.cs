// <copyright file="LiquidException.cs" company="DocFx Companion Tools">
// Copyright (c) DocFx Companion Tools. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
using System.Diagnostics.CodeAnalysis;

namespace DocFxTocGenerator.Liquid;

/// <summary>
/// Exception class for the ParserService.
/// </summary>
[ExcludeFromCodeCoverage]
public class LiquidException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LiquidException"/> class.
    /// </summary>
    public LiquidException()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="LiquidException"/> class.
    /// </summary>
    /// <param name="message">Message of exception.</param>
    public LiquidException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="LiquidException"/> class.
    /// </summary>
    /// <param name="message">Message of exception.</param>
    /// <param name="innerException">Inner exception.</param>
    public LiquidException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
