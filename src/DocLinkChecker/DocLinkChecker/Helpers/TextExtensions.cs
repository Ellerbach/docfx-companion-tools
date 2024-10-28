namespace DocLinkChecker.Helpers;

/// <summary>
/// Text extension methods. Mainly for normalizing paths and content.
/// </summary>
public static class TextExtensions
{
    /// <summary>
    /// Normalize the path to always forward-slashes.
    /// </summary>
    /// <param name="path">Path to normalize.</param>
    /// <returns>Normalized path.</returns>
    public static string NormalizePath(this string path)
    {
        return path.Replace("\\", "/");
    }

    /// <summary>
    /// Normalize content for newlines to be always just "\n".
    /// </summary>
    /// <param name="content">Content to normalize.</param>
    /// <returns>Normalized content.</returns>
    public static string NormalizeContent(this string content)
    {
        return content.Replace("\r", string.Empty);
    }
}
