using System.Text.RegularExpressions;

namespace DocLinkChecker.Helpers
{
    /// <summary>
    /// String extension methods.
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Match a string against a wildcard pattern.
        /// You can use * and ? in the wildcard pattern.
        /// </summary>
        /// <param name="str">String to match with pattern.</param>
        /// <param name="pattern">The pattern.</param>
        /// <returns>A value indicating there is a match (true) or not (false).</returns>
        public static bool Matches(this string str, string pattern)
        {
            if (string.IsNullOrEmpty(pattern))
            {
                return false;
            }

            Regex r = new Regex(WildcardToRegex(pattern), RegexOptions.IgnoreCase);
            return r.IsMatch(str);
        }

        private static string WildcardToRegex(string pattern)
        {
            return "^" + Regex.Escape(pattern).
                          Replace("\\*", ".*").
                          Replace("\\?", ".") + "$";
        }
    }
}
