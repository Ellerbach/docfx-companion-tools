namespace DocLinkChecker.Models
{
    using System.IO;
    using DocLinkChecker.Enums;

    /// <summary>
    /// Model class for hyperlink.
    /// </summary>
    public class Hyperlink : MarkdownObjectBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Hyperlink"/> class.
        /// </summary>
        /// <param name="filePath">Path of the markdown file.</param>
        /// <param name="line">Line number.</param>
        /// <param name="col">Column.</param>
        /// <param name="url">Url.</param>
        public Hyperlink(string filePath, int line, int col, string url)
            : base(filePath, line, col)
        {
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
                    if (Path.GetExtension(url).ToLower() == ".md" || Path.GetExtension(url) == string.Empty)
                    {
                        // link to an MD file or a folder
                        LinkType = HyperlinkType.Local;
                    }
                    else
                    {
                        // link to image or something like that.
                        LinkType = HyperlinkType.Resource;
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets the URL.
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this is a web link.
        /// </summary>
        public HyperlinkType LinkType { get; set; }

        /// <summary>
        /// Gets a value indicating whether this is a local link.
        /// </summary>
        public bool IsLocal
        {
            get
            {
                return LinkType == HyperlinkType.Local || LinkType == HyperlinkType.Resource;
            }
        }

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

        /// <summary>
        /// Gets the full path of the URL when it's a local reference.
        /// It's calculated relative to the file path.
        /// </summary>
        public string UrlFullPath
        {
            get
            {
                if (!IsLocal)
                {
                    return Url;
                }

                string dir = Path.GetDirectoryName(FilePath);
                return Path.GetFullPath(Path.Combine(dir, Url));
            }
        }
    }
}
