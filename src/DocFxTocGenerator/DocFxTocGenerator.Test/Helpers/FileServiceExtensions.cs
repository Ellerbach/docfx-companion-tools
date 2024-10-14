namespace DocFxTocGenerator.Test.Helpers;

internal static class FileServiceExtensions
{
    internal static string ToInternal(this string path)
    {
        return path.Replace("\\", "/").ToLowerInvariant();
    }
}
