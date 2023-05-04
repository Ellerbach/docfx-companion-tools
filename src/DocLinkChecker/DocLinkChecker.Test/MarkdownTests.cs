namespace DocLinkChecker.Test
{
    using System.Linq;
    using DocLinkChecker.Helpers;
    using DocLinkChecker.Models;
    using FluentAssertions;

    public class MarkdownTests
    {
        [Fact]
        public void FindAllLinks()
        {
            var result = MarkdownHelper.ParseMarkdownString(string.Empty, Samples.CorrectDocument, true);
            result.errors.Should().BeEmpty();

            var links = result.objects
                .OfType<Hyperlink>()
                .OrderBy(d => d.Line)
                .ThenBy(d => d.Column)
                .ToList();

            var all = Samples.CorrectDocumentCorrectLinks
                .Union(Samples.CorrectDocumentErrorLinks)
                .OrderBy(d => d.Line)
                .ThenBy(d => d.Column)
                .ToList();

            links.Count.Should().Be(all.Count);
        }

        [Fact]
        public void FindAllHeadings()
        {
            var result = MarkdownHelper.ParseMarkdownString(string.Empty, Samples.CorrectDocument, true);
            result.errors.Should().BeEmpty();

            var headings = result.objects
                .OfType<Heading>()
                .OrderBy(d => d.Line)
                .ThenBy(d => d.Column)
                .ToList();

            headings.Count.Should().Be(Samples.CorrectDocumentHeadings.Count);

            for (int i = 0; i < headings.Count; i++)
            {
                Heading heading = headings[i];
                Heading res = Samples.CorrectDocumentHeadings[i];
                heading.Line.Should().Be(res.Line);
                heading.Column.Should().Be(res.Column);
                heading.Title.Should().Be(res.Title);
                heading.Id.Should().Be(res.Id);
            }
        }

        [Fact]
        public void FindAllTables()
        {
            var result = MarkdownHelper.ParseMarkdownString(string.Empty, Samples.CorrectDocument, true);
            result.errors.Should().BeEmpty();
        }

        [Fact]
        public void FindAllTablesWithErrors()
        {
            var result = MarkdownHelper.ParseMarkdownString(string.Empty, Samples.ErrorDocument, true);
            result.errors.Count.Should().Be(Samples.ErrorDocumentErrors.Count);

            var sorted = result.errors
                .OrderBy(d => d.Line)
                .ThenBy(d => d.Column)
                .ToList();
            for (int i = 0; i < sorted.Count; i++)
            {
                MarkdownError error = sorted[i];
                MarkdownError res = Samples.ErrorDocumentErrors[i];
                error.Line.Should().Be(res.Line);
                error.Column.Should().Be(res.Column);
                error.Severity.Should().Be(res.Severity);
                error.Message.Should().Contain(res.Message);
            }
        }
    }
}