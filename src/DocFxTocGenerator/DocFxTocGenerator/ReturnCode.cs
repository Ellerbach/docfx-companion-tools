namespace DocFxTocGenerator;

/// <summary>
/// Return code for the application.
/// </summary>
public enum ReturnCode
{
    /// <summary>
    /// All went well.
    /// </summary>
    Normal = 0,

    /// <summary>
    /// A few warnings, but process completed.
    /// </summary>
    Warning = 1,

    /// <summary>
    /// An error occurred, process not completed.
    /// </summary>
    Error = 2,
}
