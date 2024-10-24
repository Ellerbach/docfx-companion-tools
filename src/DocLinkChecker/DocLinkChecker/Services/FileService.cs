using DocLinkChecker.Interfaces;
using Microsoft.Extensions.FileSystemGlobbing;

namespace DocLinkChecker.Services
{
    /// <summary>
    /// File service implementation.
    /// </summary>
    public class FileService : IFileService
    {
        /// <inheritdoc />
        public bool ExistsFileOrDirectory(string path)
        {
            return File.Exists(path) || Directory.Exists(path);
        }

        /// <inheritdoc />
        public string GetDirectory(string path)
        {
            return Path.GetDirectoryName(path);
        }

        /// <inheritdoc />
        public IEnumerable<string> GetFiles(string root, List<string> includes, List<string> excludes)
        {
            string fullRoot = GetFullPath(root);
            Matcher matcher = new();
            foreach (string folderName in includes)
            {
                matcher.AddInclude(folderName);
            }

            foreach (string folderName in excludes)
            {
                matcher.AddExclude(folderName);
            }

            return matcher.GetResultsInFullPath(fullRoot);
        }

        /// <inheritdoc/>
        public string GetRelativePath(string relativeTo, string path)
        {
            return Path.GetRelativePath(relativeTo, path);
        }

        /// <inheritdoc/>
        public string GetFullPath(string path)
        {
            return Path.GetFullPath(path);
        }

        /// <inheritdoc/>
        public void DeleteFile(string path)
        {
            File.Delete(path);
        }

        /// <inheritdoc/>
        public void DeleteFiles(string[] paths)
        {
            foreach (string path in paths)
            {
                File.Delete(path);
            }
        }
    }
}
