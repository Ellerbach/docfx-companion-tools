// Licensed to DocFX Companion Tools and contributors under one or more agreements.
// DocFX Companion Tools and contributors licenses this file to you under the MIT license.

namespace DocLinkChecker.Helpers
{
    using System;

    /// <summary>
    /// Helper methods to set the exit code.
    /// </summary>
    public static class ExitCodeHelper
    {
        private static int exitCode;

        /// <summary>
        /// Possible exit codes.
        /// </summary>
        public enum ExitCodes
        {
            /// <summary>
            /// Exit code for seccessful execution.
            /// </summary>
            OK = 0,

            /// <summary>
            /// Exit code for parsing error.
            /// </summary>
            ParsingError = 1,

            /// <summary>
            /// Exit code for incorrect table format detected.
            /// </summary>
            TableFormatError = 2,

            /// <summary>
            /// Exit code for unkonown exception.
            /// </summary>
            UnknownExceptionError = 999,
        }

        /// <summary>
        /// Gets or sets default exit code.
        /// </summary>
        public static ExitCodes ExitCode
        {
            get
            {
                return (ExitCodes)exitCode;
            }

            set
            {
                if (Enum.IsDefined(typeof(ExitCodes), value))
                {
                    exitCode = (int)value;
                }
                else
                {
                    exitCode = (int)ExitCodes.UnknownExceptionError;
                }
            }
        }
    }
}
