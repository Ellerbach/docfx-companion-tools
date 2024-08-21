namespace DocLinkChecker.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using DocLinkChecker.Enums;
    using DocLinkChecker.Models;
    using Markdig;
    using Markdig.Extensions.Tables;
    using Markdig.Syntax;
    using Markdig.Syntax.Inlines;

    /// <summary>
    /// Markdown helper methods.
    /// </summary>
    public static class MarkdownHelper
    {
        /// <summary>
        /// Get the markdown objects from the given file path.
        /// </summary>
        /// <param name="filepath">File path to read.</param>
        /// <param name="validateTable">Value indicating whether to validate the tables in the markdown.</param>
        /// <returns>List of hyperlinks and list of errors (which can be empty).</returns>
        public static async Task<(List<MarkdownObjectBase> objects, List<MarkdownError> errors)>
            ParseMarkdownFileAsync(string filepath, bool validateTable)
        {
            string markdownFilePath = Path.GetFullPath(filepath);
            string md = await File.ReadAllTextAsync(markdownFilePath);
            return ParseMarkdownString(markdownFilePath, md, validateTable);
        }

        /// <summary>
        /// Parse markdown string and get all links and headings from the markdown.
        /// If validateTable is true, it will also validate the tables.
        /// </summary>
        /// <param name="markdownFilePath">Full path name where markdown is taken from.</param>
        /// <param name="markdown">Markdown content.</param>
        /// <param name="validateTable">Value indicating whether to validate the tables in the markdown.</param>
        /// <returns>List of hyperlinks and list of errors (which can be empty).</returns>
        public static (List<MarkdownObjectBase> objects, List<MarkdownError> errors)
            ParseMarkdownString(string markdownFilePath, string markdown, bool validateTable)
        {
            List<MarkdownObjectBase> objects = new ();
            List<MarkdownError> errors = new ();

            MarkdownPipeline pipeline = new MarkdownPipelineBuilder()
                .UseAdvancedExtensions()
                .Build();
            MarkdownDocument document = Markdown.Parse(markdown, pipeline);

            // get all links
            List<Hyperlink> links = document
                .Descendants<LinkInline>()
                .Select(d => new Hyperlink(markdownFilePath, d.Line + 1, d.Column + 1, d.Url ?? string.Empty))
                .ToList();
            if (links != null)
            {
                // Support for DocFx specific links. See https://dotnet.github.io/docfx/docs/markdown.html
                // File inclusion links and Code references
                var filerefs = links.Where(x => x.Url.StartsWith("!code-", StringComparison.OrdinalIgnoreCase) ||
                                                x.Url.StartsWith("!INCLUDE", StringComparison.OrdinalIgnoreCase));
                foreach (var fileref in filerefs)
                {
                    string url = Regex.Match(fileref.Url, @"\\((.*?)\\)").Value;
                    fileref.Url = url;
                }

                // Video inclusion links
                var videorefs = links.Where(x => x.Url.StartsWith("!Video"));
                foreach (var videoref in videorefs)
                {
                    string url = Regex.Match(videoref.Url, @"!Video (.*)").Value;
                    videoref.Url = url;
                    videoref.LinkType = HyperlinkType.Webpage;
                }

                // Tabs
                var tabrefs = links.Where(x => x.Url.StartsWith("#tab/"));
                foreach (var tabref in tabrefs)
                {
                    tabref.LinkType = HyperlinkType.Tab;
                }

                objects.AddRange(links);
            }

            // get all headings
            List<Heading> headings = document
                .Descendants<HeadingBlock>()
                .Select(x =>
                {
                    string title = string.Empty;

                    var child = x.Inline.Descendants<LiteralInline>().FirstOrDefault();
                    if (child != null)
                    {
                        title = markdown.Substring(child.Span.Start, x.Span.Length - (child.Span.Start - x.Span.Start));
                    }
                    else if (x.Inline.FirstChild != null)
                    {
                        // fallback for complex headers, like "# `text with quotes`" and such
                        title = markdown.Substring(x.Inline.FirstChild.Span.Start, x.Span.Length - (x.Inline.FirstChild.Span.Start - x.Span.Start));
                    }

                    // custom generation of the id
                    string id = title.ToLower();
                    id = Regex.Replace(id, "[ _]", "-");

                    // replace all non-characters. \p[L] takes all unicode variants in account as well like ö and á
                    id = Regex.Replace(id, @"[^\p{L}0-9-]*", string.Empty);

                    return new Heading(markdownFilePath, x.Line + 1, x.Column + 1, title, id);
                })
                .ToList();

            if (headings != null)
            {
                objects.AddRange(headings);
            }

            // if we have to validate tables, get all and validate
            if (validateTable)
            {
                List<Table> tables = document.Descendants<Table>().ToList();
                if (tables != null)
                {
                    foreach (Table table in tables)
                    {
                        // validate the table and store the result in the list
                        var result = ValidatePipeTableWithText(markdownFilePath, markdown, table);
                        errors.AddRange(result.errors);
                    }
                }
            }

            return (objects, errors);
        }

        /// <summary>
        /// Validate tables for consistent number of columns, use of pipe characters and
        /// a valid separator line.
        /// </summary>
        /// <param name="markdownFilePath">Markdown file path.</param>
        /// <param name="markdown">Markdown content.</param>
        /// <param name="table">Table.</param>
        /// <returns>Pipetable definition and list of (possible) errors.</returns>
        private static (PipeTable table, List<MarkdownError> errors)
            ValidatePipeTable(string markdownFilePath, string markdown, Table table)
        {
            PipeTable pipeTable = new (markdownFilePath, table.Line, table.Column);
            List<MarkdownError> errors = new ();
            int nrCols = -1;

            // get all table rows. This will skip the seperator line in markdig.
            List<TableRow> rows = table.Descendants<TableRow>().ToList();
            foreach (TableRow row in rows)
            {
                // determine columns from the actual text of the row
                int cols = markdown.Substring(row.Span.Start, row.Span.Length).Split("|").Length;
                if (nrCols == -1)
                {
                    // if this is the first row, it determines the size of the table
                    nrCols = cols;
                }

                if (cols != nrCols)
                {
                    errors.Add(
                        new MarkdownError(
                            markdownFilePath,
                            row.Line + 1,
                            row.Column,
                            MarkdownErrorSeverity.Error,
                            $"All rows in this table must have {nrCols} columns."));
                }

                // we want to get the text of the row, but not more than 2 characters after the row
                // we need the two extra characters to get the closing '|', as markdig strips that off in the object
                // but we need to maximize to the length of the markdown string.
                int len = row.Span.Length + Math.Min((markdown.Length - row.Span.End) - 1, 2);
                string rowText = markdown.Substring(row.Span.Start, len).Replace("\n", string.Empty).Replace("\r", string.Empty).Trim();

                if (!rowText.EndsWith('|'))
                {
                    errors.Add(
                        new MarkdownError(
                            markdownFilePath,
                            row.Line + 1,
                            row.Span.Length,
                            MarkdownErrorSeverity.Error,
                            "All rows in a table must start and end with a pipe character ('|')."));
                }
            }

            // now parse the raw table markdown to check the seperator-line (second)
            // This should be something like "|---|---|---|"
            string tableContent = markdown.Substring(table.Span.Start, table.Span.Length);
            string[] lines = tableContent.Split('\n');
            if (lines.Length > 1)
            {
                // we remove newlines and the opening and closing '|' to properly determine the number of columns
                string seperators = lines[1].Replace("\n", string.Empty).Replace("\r", string.Empty).Trim().TrimStart('|').TrimEnd('|');
                string[] columnEntries = seperators.Split("|");
                if (columnEntries.Length != nrCols)
                {
                    errors.Add(
                        new MarkdownError(
                            markdownFilePath,
                            table.Line + 2,
                            table.Column,
                            MarkdownErrorSeverity.Error,
                            $"All rows in this table must have {nrCols} columns."));
                }

                // check every column to have at least three '-' characters to make it work properly in Azure DevOps and such
                foreach (string entry in columnEntries)
                {
                    if (!entry.Contains("---"))
                    {
                        errors.Add(
                            new MarkdownError(
                                markdownFilePath,
                                table.Line + 2,
                                table.Column,
                                MarkdownErrorSeverity.Error,
                                $"Second line of a table should have at least 3 pipe characters ('---') per column."));
                    }
                }
            }

            return (pipeTable, errors);
        }

        /// <summary>
        /// Validate tables for consistent number of columns, use of pipe characters and
        /// a valid separator line.
        /// NOTE:
        /// This is the implementation based on just text parsing, not using markdig objects.
        /// The reason for this implementation is that we found markdig tables to be inconsistent
        /// in the spans it returns, not always returning the full table.
        /// In this implementation we chose to start with the indicated starting position, but
        /// then loop over all lines by newline until we have an empty line or EOF.
        /// </summary>
        /// <param name="markdownFilePath">Markdown file path.</param>
        /// <param name="markdown">Markdown content.</param>
        /// <param name="table">Table.</param>
        /// <returns>Pipetable definition and list of (possible) errors.</returns>
        private static (PipeTable table, List<MarkdownError> errors)
            ValidatePipeTableWithText(string markdownFilePath, string markdown, Table table)
        {
            PipeTable pipeTable = null;
            int start = 0;
            int len = 0;
            string line = string.Empty;
            List<MarkdownError> errors = new ();

            try
            {
                pipeTable = new (markdownFilePath, table.Line, table.Column);

                start = Math.Max(table.Span.Start, 0);
                while (start > 0 && markdown.Substring(start, 1) != "\n")
                {
                    start--;
                }

                if (markdown.IndexOf('\r') >= 0)
                {
                    len = markdown.IndexOf('\r', start) - start;
                }
                else if (markdown.IndexOf('\n') >= 0)
                {
                    len = markdown.IndexOf('\n', start) - start;
                }

                if (len > 0)
                {
                    line = markdown.Substring(start, len);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"PipeTable error\nPATH: {markdownFilePath} MD: {markdown.Substring(0, 10)}...  TABLE: line={table.Line} col={table.Column} start={table.Span.Start}\n{ex}");
            }

            int row = 0;
            int nrCols = -1;
            while (!string.IsNullOrEmpty(line))
            {
                // check the row definition. Make sure escaped seperators are ignored.
                string rowtext = Regex.Replace(line.Trim(), @"\\\|", string.Empty);
                if (rowtext.StartsWith("|"))
                {
                    // we cannot use TrimStart, because it will remove all '|' characters in stead of just one.
                    rowtext = rowtext.Substring(1);
                }

                if (rowtext.EndsWith("|"))
                {
                    // we cannot use TrimEnd, because it will remove all '|' characters in stead of just one.
                    rowtext = rowtext.Substring(0, rowtext.Length - 1);
                }

                string[] columnEntries = rowtext.Split("|");
                int cols = columnEntries.Length;

                // determine columns from the actual text of the row
                if (nrCols == -1)
                {
                    // if this is the first row, it determines the size of the table
                    nrCols = cols;
                }

                // validate number of columns
                if (cols != nrCols)
                {
                    errors.Add(
                        new MarkdownError(
                            markdownFilePath,
                            table.Line + 1 + row,
                            1,
                            MarkdownErrorSeverity.Error,
                            $"All rows in this table must have {nrCols} columns."));
                }

                if (row == 1)
                {
                    // now parse the raw table markdown to check the seperator-line (second)
                    // This should be something like "|---|---|---|"

                    // check every column to have at least three '-' characters to make it work properly in Azure DevOps and such
                    foreach (string entry in columnEntries)
                    {
                        if (!entry.Contains("---"))
                        {
                            errors.Add(
                                new MarkdownError(
                                    markdownFilePath,
                                    table.Line + 2,
                                    table.Column,
                                    MarkdownErrorSeverity.Error,
                                    $"Second line of a table should have at least 3 dashes ('---') per column."));
                        }
                    }
                }
                else
                {
                    if (!line.EndsWith('|'))
                    {
                        errors.Add(
                            new MarkdownError(
                                markdownFilePath,
                                table.Line + 1 + row,
                                line.Length - 1,
                                MarkdownErrorSeverity.Error,
                                "All rows in a table must start and end with a pipe character ('|')."));
                    }
                }

                start += len + 2;   // add 2 for the \r\n
                if (start >= markdown.Length)
                {
                    // end of file
                    break;
                }

                len = markdown.IndexOf('\r', start) - start;
                line = markdown.Substring(start, len);
                row++;
            }

            return (pipeTable, errors);
        }
    }
}
