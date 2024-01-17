namespace DocLinkChecker.Test
{
    using System.Linq;
    using Bogus;
    using DocLinkChecker.Helpers;
    using DocLinkChecker.Models;
    using DocLinkChecker.Test.Helpers;
    using FluentAssertions;

    public class MarkdownTests
    {
        private string _correctDocument = string.Empty
                .AddHeading("Sample General Document", 1)
                .AddParagraphs(1).AddLink("https://loremipsum.io/generator/?n=5&t=p")
                .AddParagraphs(1).AddLink("./another-sample.md#third1-header")
                .AddParagraphs(1).AddLink("http://ww1.microchip.com/downloads/en/devicedoc/21295c.pdf")
                .AddNewLine()
                .AddHeading("Resource link", 2)
                .AddResourceLink("./images/nature.jpeg")
                .AddHeading("Next paragraph with link", 2)
                .AddParagraphs(1).AddLink("./another-sample.md")
                .AddNewLine()
                .AddHeading("Another resource link", 2)
                .AddResourceLink("images/another-image.png")
                .AddHeading("Yet another paragraph with link", 2)
                .AddParagraphs(1).AddLink("https://www.hanselman.com/blog/RemoteDebuggingWithVSCodeOnWindowsToARaspberryPiUsingNETCoreOnARM.aspx")
                .AddHeading("Now we have table", 2)
                .AddTableStart(3)
                .AddTableRow("Microsoft", "<https://www.microsoft.com>", string.Empty.AddLink("https://blogs.microsoft.com/"))
                .AddTableRow(".NET Foundation", "<https://dotnetfoundation.org/>", string.Empty.AddLink("https://dotnetfoundation.org/about/faq"))
                .AddTableRow("Github", "<https://github.com/> ", string.Empty.AddLink("https://github.blog/"))
                .AddNewLine()
                .AddParagraphs(2);

        private string _errorDocument = string.Empty
                .AddHeading("Sample General Document", 1)
                .AddParagraphs(1).AddLink("https://loremipsum.io/generator/?n=5&t=p")
                .AddParagraphs(1).AddLink("./another-sample.md#third1-header")
                .AddParagraphs(1).AddLink("https://xyzdoesnotexist.com/blah")
                .AddNewLine()
                .AddResourceLink("./images/nature.jpeg")
                .AddParagraphs(1).AddLink("./another-sample.md")
                .AddNewLine()
                .AddResourceLink("images/another-image.png")
                .AddParagraphs(1).AddLink("https://www.hanselman.com/blog/RemoteDebuggingWithVSCodeOnWindowsToARaspberryPiUsingNETCoreOnARM.aspx")
                .AddNewLine()
                .AddTableStart(3)
                // wrong number of columns
                .AddTableRow("Microsoft", string.Empty.AddLink("https://blogs.microsoft.com/"))
                // add line without end pipe for the table
                .AddRawMarkdown($"| .NET Foundation | <https://dotnetfoundation.org/> | [som link](https://dotnetfoundation.org/) => no end column pipe ")
                .AddNewLine()
                .AddTableRow("Github", "<https://github.com/> ", string.Empty.AddLink("https://github.blog/"))
                .AddNewLine()
                .AddParagraphs(2);

        private string _codeLinkingDocument = string.Empty
                .AddHeading("Code Linking Document", 1)
                .AddParagraphs(1).AddCodeLink("Code", "scripts/foo.cs")
                .AddParagraphs(1).AddCodeLink("Code", "scripts/foo.cs?name=Bravo")
                .AddNewLine()
                .AddParagraphs(1).AddCodeLink(string.Empty, "scripts/bar.cs#starting")
                .AddParagraphs(1).AddCodeLink(string.Empty, "scripts/bar.cs#L3-5")
                .AddParagraphs(1).AddCodeLink(string.Empty, "scripts/bar.cs?highlight=2,5-7,9-)")
                .AddNewLine()
                .AddParagraphs(2);

        [Fact]
        public void FindAllLinks()
        {
            var result = MarkdownHelper.ParseMarkdownString(string.Empty, _correctDocument, true);
            result.errors.Should().BeEmpty();

            var links = result.objects
                .OfType<Hyperlink>()
                .OrderBy(d => d.Line)
                .ThenBy(d => d.Column)
                .ToList();

            links.Count.Should().Be(10);
        }

        [Fact]
        public void FindAllHeadings()
        {
            var result = MarkdownHelper.ParseMarkdownString(string.Empty, _correctDocument, true);
            result.errors.Should().BeEmpty();

            var headings = result.objects
                .OfType<Heading>()
                .ToList();

            headings.Count.Should().Be(6);
        }

        [Fact]
        public void FindAllHeadingsWithUnicodeCharacters()
        {
            string markdown = string.Empty
                .AddHeading("Test Unicode Characters", 1)
                .AddParagraphs(1).AddLink("#")
                .AddHeading("abcdefghijklmnopqrstuvwxyz 0123456789", 2)
                .AddParagraphs(1)
                .AddHeading("ABCDEFGHIJKLMNOPQRSTUVWXYZ 0123456789", 2)
                .AddParagraphs(1)
                .AddHeading("UNICODE-!@#$%^&*+=~`<>,.?/:;€|Æäßéóčúįǯ-CHARS", 2)
                .AddParagraphs(1);

            var result = MarkdownHelper.ParseMarkdownString(string.Empty, markdown, true);

            var headings = result.objects
                .OfType<Heading>()
                .ToList();

            headings.Count.Should().Be(4);
            headings[1].Id.Should().Be("abcdefghijklmnopqrstuvwxyz-0123456789");
            headings[2].Id.Should().Be("abcdefghijklmnopqrstuvwxyz-0123456789");
            headings[3].Id.Should().Be("unicode-æäßéóčúįǯ-chars");
        }

        [Fact]
        public void FindAllTables()
        {
            var result = MarkdownHelper.ParseMarkdownString(string.Empty, _correctDocument, true);
            result.errors.Should().BeEmpty();
        }

        [Fact]
        public void FindAllTablesWithErrors()
        {
            var result = MarkdownHelper.ParseMarkdownString(string.Empty, _errorDocument, true);
            result.errors.Count.Should().Be(5);
        }

        [Fact]
        public void FindAllCodeLinks()
        {
            var result = MarkdownHelper.ParseMarkdownString(string.Empty, _codeLinkingDocument, false);
            result.errors.Should().BeEmpty();
        }
    }
}