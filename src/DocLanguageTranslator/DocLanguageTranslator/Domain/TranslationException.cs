// Licensed to DocFX Companion Tools and contributors under one or more agreements.
// DocFX Companion Tools and contributors licenses this file to you under the MIT license.

namespace DocLanguageTranslator.Domain;

/// <summary>
/// Represents errors that occur during translation operations.
/// </summary>
internal class TranslationException : Exception
{
    /// <summary>
    /// Gets the error code associated with this translation exception.
    /// </summary>
    public int ErrorCode { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="TranslationException"/> class with a specified error message and error code.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="errorCode">The error code that identifies the specific translation error.</param>
    public TranslationException(string message, int errorCode)
        : base(message)
    {
        ErrorCode = errorCode;
    }
}
