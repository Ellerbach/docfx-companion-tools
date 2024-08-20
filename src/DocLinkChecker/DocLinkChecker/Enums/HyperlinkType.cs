namespace DocLinkChecker.Enums
{
    /// <summary>
    /// Enumeration of hyperlink types.
    /// </summary>
    public enum HyperlinkType
    {
        /// <summary>
        /// Local file.
        /// </summary>
        Local,

        /// <summary>
        /// A web page (http or https).
        /// </summary>
        Webpage,

        /// <summary>
        /// A download link (ftp or ftps).
        /// </summary>
        Ftp,

        /// <summary>
        /// Mail address (mailto).
        /// </summary>
        Mail,

        /// <summary>
        /// A cross reference (xref).
        /// </summary>
        CrossReference,

        /// <summary>
        /// A local resource, like an image.
        /// </summary>
        Resource,

        /// <summary>
        /// A tab - DocFx special. See https://dotnet.github.io/docfx/docs/markdown.html?tabs=linux%2Cdotnet#tabs.
        /// </summary>
        Tab,

        /// <summary>
        /// Empty link.
        /// </summary>
        Empty,
    }
}
