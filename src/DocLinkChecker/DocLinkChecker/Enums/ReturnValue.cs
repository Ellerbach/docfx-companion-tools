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
        /// All went well.
        /// </summary>
        OK = 0,

        /// <summary>
        /// Command errors in the commandline.
        /// </summary>
        CommandError = 1,

        /// <summary>
        /// Errors in the parameters of the commandline.
        /// </summary>
        ParameterErrors = 2,

        /// <summary>
        /// Errors in the configuration by file.
        /// </summary>
        ConfigurationFileErrors = 3,

        /// <summary>
        /// Processing errors of the files.
        /// </summary>
        ProcessingErrors = 1000,
    }
}
