using System.Diagnostics.CodeAnalysis;

namespace DocFxTocGenerator.FileService;

/// <summary>
/// File path extension methods.
/// </summary>
[ExcludeFromCodeCoverage]
public static class FilePathExtensions
{
    /// <summary>
    /// Normalize the path to have a common notation of directory separators.
    /// This is needed when used in Equal() methods and such.
    /// </summary>
    /// <param name="path">Path to normalize.</param>
    /// <returns>Normalized path.</returns>
    public static string NormalizePath(this string path)
    {
        return path.Replace("\\", "/");
    }

    /// <summary>
    /// Normalize the content. This is used to make sure we always
    /// have "\n" only for new lines. Mostly used by the test mocks.
    /// </summary>
    /// <param name="content">Content to normalize.</param>
    /// <returns>Normalized content.</returns>
    public static string NormalizeContent(this string content)
    {
        return content.Replace("\r", string.Empty);
    }
}
