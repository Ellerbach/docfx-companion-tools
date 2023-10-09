namespace DocLinkChecker.Models
{
    using DocLinkChecker.Enums;

    /// <summary>
    /// Model class for validation error.
    /// </summary>
    public class MarkdownError
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MarkdownError"/> class.
        /// </summary>
        /// <param name="markdownFilePath">Markdown file path.</param>
        /// <param name="line">Line number.</param>
        /// <param name="column">Column number.</param>
        /// <param name="severity">Markdown error severity.</param>
        /// <param name="message">Message.</param>
        public MarkdownError(string markdownFilePath, int line, int column, MarkdownErrorSeverity severity, string message)
        {
            MarkdownFilePath = markdownFilePath;
            Line = line;
            Column = column;
            Severity = severity;
            Message = message;
        }

        /// <summary>
        /// Gets or sets the markdown file path.
        /// </summary>
        public string MarkdownFilePath { get; set; }

        /// <summary>
        /// Gets or sets the line number.
        /// </summary>
        public int Line { get; set; }

        /// <summary>
        /// Gets or sets the column.
        /// </summary>
        public int Column { get; set; }

        /// <summary>
        /// Gets or sets the error type.
        /// </summary>
        public MarkdownErrorSeverity Severity { get; set; }

        /// <summary>
        /// Gets or sets the message.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Returns the location in a formatted string.
        /// </summary>
        /// <param name="relativeTo">Relative to path.</param>
        /// <returns>Formatted location.</returns>
        public string GetLocationString(string relativeTo = "")
        {
            string location = string.Empty;
            if (Line > 0 && Column > 0)
            {
                location = $"{Line}:{Column}";
            }

            string path = MarkdownFilePath;
            if (!string.IsNullOrEmpty(relativeTo))
            {
                path = MarkdownFilePath.Replace(relativeTo, ".");
            }

            return $"{path} {location}";
        }

        /// <summary>
        /// Returns the message for this error.
        /// </summary>
        /// <returns>Formatted message.</returns>
        public override string ToString()
        {
            return $"{MarkdownFilePath} {Line}:{Column}\n***{Severity.ToString().ToUpperInvariant()} {Message}";
        }
    }
}
