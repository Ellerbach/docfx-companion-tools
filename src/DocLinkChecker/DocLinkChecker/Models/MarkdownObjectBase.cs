namespace DocLinkChecker.Models
{
    /// <summary>
    /// Base class for markdown objects.
    /// </summary>
    public abstract class MarkdownObjectBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MarkdownObjectBase"/> class.
        /// </summary>
        public MarkdownObjectBase()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MarkdownObjectBase"/> class.
        /// </summary>
        /// <param name="filePath">Path of the markdown file.</param>
        /// <param name="line">Line number.</param>
        /// <param name="pos">Column.</param>
        public MarkdownObjectBase(string filePath, int line, int pos)
        {
            FilePath = filePath;
            Line = line;
            Column = pos;
        }

        /// <summary>
        /// Gets or sets the file path name of the markdown file.
        /// </summary>
        public string FilePath { get; set; }

        /// <summary>
        /// Gets or sets the line number in the file.
        /// </summary>
        public int Line { get; set; }

        /// <summary>
        /// Gets or sets the column in the file.
        /// </summary>
        public int Column { get; set; }
    }
}
