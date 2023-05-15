namespace DocLinkChecker.Enums
{
    /// <summary>
    /// Application return values.
    /// </summary>
    public enum ReturnValue
    {
        /// <summary>
        /// We're processing, so no return value yet.
        /// </summary>
        Processing = -1,

        /// <summary>
        /// Successful validation. No errors, no warnings.
        /// </summary>
        Success = 0,

        /// <summary>
        /// Errors in the commandline.
        /// </summary>
        CommandError = 1,

        /// <summary>
        /// Errors in the configuration by file.
        /// </summary>
        ConfigurationFileErrors = 3,

        /// <summary>
        /// There were only warnings in processing the files, no errors.
        /// </summary>
        WarningsOnly = 1000,

        /// <summary>
        /// There were errors in processing the files.
        /// </summary>
        Errors = 1001,
    }
}
