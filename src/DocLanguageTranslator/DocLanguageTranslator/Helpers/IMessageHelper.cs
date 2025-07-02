// Licensed to DocFX Companion Tools and contributors under one or more agreements.
// DocFX Companion Tools and contributors licenses this file to you under the MIT license.
namespace DocFXLanguageGenerator.Helpers
{
    /// <summary>
    /// Interface for handling different types of messages in the application.
    /// Provides methods for logging errors, verbose information, and warnings.
    /// </summary>
    public interface IMessageHelper
    {
        /// <summary>
        /// Logs an error message.
        /// </summary>
        /// <param name="message">The error message to be logged.</param>
        void Error(string message);

        /// <summary>
        /// Logs a verbose (detailed) information message.
        /// </summary>
        /// <param name="message">The verbose message to be logged.</param>
        void Verbose(string message);

        /// <summary>
        /// Logs a warning message.
        /// </summary>
        /// <param name="message">The warning message to be logged.</param>
        void Warning(string message);
    }
}
