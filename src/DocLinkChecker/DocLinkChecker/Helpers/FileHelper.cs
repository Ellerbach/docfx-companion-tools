namespace DocLinkChecker.Helpers
{
    using System.IO;

    /// <summary>
    /// File helper methods.
    /// </summary>
    public static class FileHelper
    {
        /// <summary>
        /// Get the given path as relative to.
        /// </summary>
        /// <param name="path">Path to make relative.</param>
        /// <param name="relativeTo">Relative to.</param>
        /// <returns>Relative path.</returns>
        public static string GetRelativePath(string path, string relativeTo)
        {
            string fullPath = Path.GetFullPath(path);
            return fullPath.Replace(relativeTo, ".");
        }
    }
}
