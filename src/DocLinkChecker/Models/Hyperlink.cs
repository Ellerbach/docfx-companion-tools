namespace DocLinkChecker.Models
{
    using DocLinkChecker.Enums;

    /// <summary>
    /// Model class for hyperlink.
    /// </summary>
    public class Hyperlink
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Hyperlink"/> class.
        /// </summary>
        /// <param name="fullpathname">Full path of the file.</param>
        /// <param name="linenum">Line number.</param>
        /// <param name="url">Url.</param>
        public Hyperlink(string fullpathname, int linenum, string url)
        {
            FullPathName = fullpathname;
            LineNum = linenum;
            Url = url;

            LinkType = HyperlinkType.Empty;
            if (!string.IsNullOrWhiteSpace(url))
            {
                if (url.StartsWith("https://") || url.StartsWith("http://"))
                {
                    LinkType = HyperlinkType.Webpage;
                }
                else if (url.StartsWith("ftps://") || url.StartsWith("ftp://"))
                {
                    LinkType = HyperlinkType.Ftp;
                }
                else if (url.StartsWith("mailto:"))
                {
                    LinkType = HyperlinkType.Mail;
                }
                else if (url.StartsWith("xref:"))
                {
                    LinkType = HyperlinkType.CrossReference;
                }
                else
                {
                    LinkType = HyperlinkType.Local;
                }
            }
        }

        /// <summary>
        /// Gets or sets the full path name of the file containing the link.
        /// </summary>
        public string FullPathName { get; set; }

        /// <summary>
        /// Gets or sets the line number in the file.
        /// </summary>
        public int LineNum { get; set; }

        /// <summary>
        /// Gets or sets the URL.
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this is a web link.
        /// </summary>
        public HyperlinkType LinkType { get; set; }

        /// <summary>
        /// Gets a value indicating whether this is a web link.
        /// </summary>
        public bool IsWeb
        {
            get
            {
                return LinkType == HyperlinkType.Webpage || LinkType == HyperlinkType.Ftp;
            }
        }
    }
}
