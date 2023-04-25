// Licensed to DocFX Companion Tools and contributors under one or more agreements.
// DocFX Companion Tools and contributors licenses this file to you under the MIT license.
namespace DocLinkChecker.Services
{
    using System;

    /// <summary>
    /// Custom console logger. It's a wrapper around the Console class.
    /// It handles output colors and only shows verbose when it is enabled.
    /// </summary>
    public class CustomConsoleLogger
    {
        private readonly bool _verbose;

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomConsoleLogger"/> class.
        /// </summary>
        /// <param name="verbose">The verbose switch.</param>
        public CustomConsoleLogger(bool verbose)
        {
            _verbose = verbose;
        }

        /// <summary>
        /// Helper method for verbose messages.
        /// </summary>
        /// <param name="message">Message to show in verbose mode.</param>
        public void Output(string message)
        {
            Console.WriteLine(message);
        }

        /// <summary>
        /// Helper method for verbose messages. Only displays when verbose is enabled.
        /// </summary>
        /// <param name="message">Message to show in verbose mode.</param>
        public void Verbose(string message)
        {
            if (_verbose)
            {
                Console.WriteLine(message);
            }
        }

        /// <summary>
        /// Helper method for warning messages.
        /// </summary>
        /// <param name="message">Message to show in verbose mode.</param>
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
        public void Error(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(message);
            Console.ResetColor();
        }
    }
}
