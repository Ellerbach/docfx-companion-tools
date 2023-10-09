namespace DocLinkChecker.Interfaces
{
    /// <summary>
    /// Interface for the custom console logger.
    /// </summary>
    public interface ICustomConsoleLogger
    {
        /// <summary>
        /// Helper method for verbose messages.
        /// </summary>
        /// <param name="message">Message to show in verbose mode.</param>
        void Output(string message);

        /// <summary>
        /// Helper method for verbose messages. Only displays when verbose is enabled.
        /// </summary>
        /// <param name="message">Message to show in verbose mode.</param>
        void Verbose(string message);

        /// <summary>
        /// Helper method for warning messages.
        /// </summary>
        /// <param name="message">Message to show in verbose mode.</param>
        void Warning(string message);

        /// <summary>
        /// Helper method for error messages.
        /// </summary>
        /// <param name="message">Message to show in verbose mode.</param>
        void Error(string message);
    }
}
