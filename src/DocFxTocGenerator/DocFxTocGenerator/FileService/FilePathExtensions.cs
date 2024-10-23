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
}
