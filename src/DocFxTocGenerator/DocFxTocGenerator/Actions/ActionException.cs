// <copyright file="ActionException.cs" company="DocFx Companion Tools">
// Copyright (c) DocFx Companion Tools. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
using System.Diagnostics.CodeAnalysis;

namespace DocFxTocGenerator.Actions;

/// <summary>
/// Exception class for the ParserService.
/// </summary>
[ExcludeFromCodeCoverage]
public class ActionException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ActionException"/> class.
    /// </summary>
    public ActionException()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ActionException"/> class.
    /// </summary>
    /// <param name="message">Message of exception.</param>
    public ActionException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ActionException"/> class.
    /// </summary>
    /// <param name="message">Message of exception.</param>
    /// <param name="innerException">Inner exception.</param>
    public ActionException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
