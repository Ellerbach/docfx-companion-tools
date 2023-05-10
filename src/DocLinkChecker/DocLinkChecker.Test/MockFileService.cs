namespace DocLinkChecker.Test
{
    using DocLinkChecker.Interfaces;
    using System.Collections.Generic;

    public class MockFileService : IFileService
    {
        public bool Exists { get; set; } = false;
        public List<string> Files { get; set; } = new ();

        public void DeleteFile(string path)
        {
        }

        public void DeleteFiles(string[] paths)
        {
        }

        public bool ExistsFileOrDirectory(string path)
        {
            return Exists;
        }

        public string GetDirectory(string path)
        {
            return System.IO.Path.GetDirectoryName(path);
        }

        public IEnumerable<string> GetFiles(string root, List<string> includes, List<string> excludes)
        {
            return Files;
        }

        public string GetFullPath(string path)
        {
            return path;
        }

        public string GetRelativePath(string relativeTo, string path)
        {
            return path;
        }
    }
}
