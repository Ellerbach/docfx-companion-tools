namespace DocLinkChecker.Enums
{
    /// <summary>
    /// Enumeration of hyperlink types.
    /// </summary>
    public enum RelativeLinkType
    {
        /// <summary>
        /// Relative links to all content is allowed.
        /// </summary>
        All,

        /// <summary>
        /// Relative links are only allowed within the same docs hierarchy.
        /// </summary>
        SameDocsHierarchyOnly,

        /// <summary>
        /// Relative links are allowed to content in any docs hierarchy.
        /// This means that the full path of the content is checked that it has /docs
        /// somewhere in the path.
        /// </summary>
        AnyDocsHierarchy,
    }
}
