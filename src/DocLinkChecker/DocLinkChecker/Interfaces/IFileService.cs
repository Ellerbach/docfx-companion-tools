namespace DocLinkChecker.Interfaces
{
    using System.Collections.Generic;

    /// <summary>
    /// Interface for file service.
    /// </summary>
    public interface IFileService
    {
        /// <summary>
        /// Check if the given path exists as file or directory.
        /// </summary>
        /// <param name="path">Path to check.</param>
        /// <returns>A value indicating whether the path exists.</returns>
        bool ExistsFileOrDirectory(string path);

        /// <summary>
        /// Get files with the Glob File Pattern.
        /// </summary>
        /// <param name="root">Root path.</param>
        /// <param name="includes">Include patterns.</param>
        /// <param name="excludes">Exclude patterns.</param>
        /// <returns>List of files.</returns>
        IEnumerable<string> GetFiles(string root, List<string> includes, List<string> excludes);

        /// <summary>
        /// Get the directory of the given path.
        /// </summary>
        /// <param name="path">Path.</param>
        /// <returns>Directory.</returns>
        string GetDirectory(string path);

        /// <summary>
        /// Get the full path of the given path.
        /// </summary>
        /// <param name="path">Relative path.</param>
        /// <returns>Full path.</returns>
        string GetFullPath(string path);

        /// <summary>
        /// Get the relative path of the given path, relative to the relativeTo path.
        /// </summary>
        /// <param name="relativeTo">Relative to this folder.</param>
        /// <param name="path">Path to determine the relative path for.</param>
        /// <returns>Relative path.</returns>
        string GetRelativePath(string relativeTo, string path);

        /// <summary>
        /// Delete the given file.
        /// </summary>
        /// <param name="path">Path of file.</param>
        void DeleteFile(string path);

        /// <summary>
        /// Delete the list of files.
        /// </summary>
        /// <param name="paths">Paths of files.</param>
        void DeleteFiles(string[] paths);
    }
}
