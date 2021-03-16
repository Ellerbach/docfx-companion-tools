// Licensed to DocFX Companion Tools and contributors under one or more agreements.
// DocFX Companion Tools and contributors licenses this file to you under the MIT license.

namespace TocDocFxCreation.Helpers
{
    using System;
    using TocDocFxCreation.Domain;

    /// <summary>
    /// Helper methods to write messages to the console.
    /// </summary>
    public class MessageHelper
    {
        private readonly CommandlineOptions options;

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageHelper"/> class.
        /// </summary>
        /// <param name="options">Command line options.</param>
        public MessageHelper(CommandlineOptions options)
        {
            this.options = options;
        }

        /// <summary>
        /// Helper method for verbose messages.
        /// </summary>
        /// <param name="message">Message to show in verbose mode.</param>
        public void Verbose(string message)
        {
            if (this.options == null || this.options.Verbose)
            {
                Console.WriteLine(message);
            }
        }

        /// <summary>
        /// Helper method for warning messages.
        /// </summary>
        /// <param name="message">Message to show in verbose mode.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "We want same access for all methods.")]
        public void Warning(string message)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(message);
            Console.ResetColor();
        }

        /// <summary>
        /// Helper method for error messages.
        /// </summary>
        /// <param name="message">Message to show in verbose mode.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "We want same access for all methods.")]
        public void Error(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(message);
            Console.ResetColor();
        }
    }
}
