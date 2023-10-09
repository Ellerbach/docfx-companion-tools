namespace DocLinkChecker.Models
{
    /// <summary>
    /// Pipe table in markdown file.
    /// </summary>
    public class PipeTable : MarkdownObjectBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PipeTable"/> class.
        /// </summary>
        /// <param name="filePath">File path of markdown file.</param>
        /// <param name="line">Line number.</param>
        /// <param name="pos">Column.</param>
        public PipeTable(string filePath, int line, int pos)
            : base(filePath, line, pos)
        {
        }
    }
}
