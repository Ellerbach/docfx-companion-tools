namespace DocLinkChecker.Models
{
    /// <summary>
    /// Heading in markdown file.
    /// </summary>
    public class Heading : MarkdownObjectBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Heading"/> class.
        /// </summary>
        /// <param name="filePath">File path of markdown file.</param>
        /// <param name="line">Line number.</param>
        /// <param name="pos">Column.</param>
        /// <param name="title">Title.</param>
        /// <param name="id">Id.</param>
        public Heading(string filePath, int line, int pos, string title, string id)
            : base(filePath, line, pos)
        {
            Title = title;
            Id = id;
        }

        /// <summary>
        /// Gets or sets the title of the heading.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the id of the heading.
        /// </summary>
        public string Id { get; set; }
    }
}
