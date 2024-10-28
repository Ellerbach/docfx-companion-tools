using System;

namespace DocLinkChecker.Test.Helpers
{
    internal static class MarkdownExtensions
    {
        internal const string Newline = "\n";

        internal static string AddHeading(this string s, string title, int level)
        {
            string content = $"{new string('#', level)} {title}" + Newline + Newline;
            if (string.IsNullOrEmpty(s))
            {
                return content;
            }
            return s + Newline + content;
        }

        internal static string AddParagraphs(this string s, int count = 1)
        {
            Faker faker = new Faker();
            string content = (count == 1 ? faker.Lorem.Paragraph() : faker.Lorem.Paragraphs(count)) + Newline;
            if (string.IsNullOrEmpty(s))
            {
                return content;
            }
            return s + Newline + content;
        }

        internal static string AddResourceLink(this string s, string url)
        {
            Faker faker = new Faker();
            string content = $" ![some resource {faker.Random.Int(1)}]({url})" + Newline;
            if (string.IsNullOrEmpty(s))
            {
                return content;
            }
            return s + Newline + content;
        }

        internal static string AddLink(this string s, string url)
        {
            Faker faker = new Faker();
            string content = $" [some link {faker.Random.Int(1)}]({url})" + Newline;
            if (string.IsNullOrEmpty(s))
            {
                return content;
            }
            return s + Newline + content;
        }

        internal static string AddCodeLink(this string s, string name, string url)
        {
            Faker faker = new Faker();
            string content = $" [!code-csharp[{name}]({url})]" + Newline;
            if (string.IsNullOrEmpty(s))
            {
                return content;
            }
            return s + Newline + content;
        }

        internal static string AddTableStart(this string s, int columns = 3)
        {
            Faker faker = new Faker();
            string content = "|";
            for (int col = 0; col < columns; col++)
            {
                content += $" {faker.Lorem.Sentence} |";
            }
            content += Newline;
            for (int col = 0; col < columns; col++)
            {
                content += $" --- |";
            }
            content += Newline;
            if (string.IsNullOrEmpty(s))
            {
                return content;
            }
            return s + Newline + content;
        }

        internal static string AddTableRow(this string s, params string[] columns)
        {
            Faker faker = new Faker();
            string content = "|";
            foreach (string col in columns)
            {
                content += $" {col} |";
            }
            content += Newline;
            if (string.IsNullOrEmpty(s))
            {
                return content;
            }
            return s + Newline + content;
        }

        internal static string AddNewLine(this string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                return Newline;
            }
            return s + Newline;
        }

        internal static string AddRawMarkdown(this string s, string markdown)
        {
            if (string.IsNullOrEmpty(s))
            {
                return markdown;
            }
            return s + markdown;
        }

        internal static string AddRawContent(this string s, string content)
        {
            if (string.IsNullOrEmpty(s))
            {
                return content;
            }
            return s + content;
        }
    }
}
