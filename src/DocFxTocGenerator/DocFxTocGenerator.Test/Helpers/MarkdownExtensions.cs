using Bogus;

namespace DocFxTocGenerator.Test.Helpers;

internal static class MarkdownExtensions
{
    internal static string AddHeading(this string s, string title, int level)
    {
        var content = $"{new string('#', level)} {title}" + Environment.NewLine + Environment.NewLine;
        if (string.IsNullOrEmpty(s))
        {
            return content;
        }
        return s + Environment.NewLine + content;
    }

    internal static string AddParagraphs(this string s, int count = 1)
    {
        var faker = new Faker();
        var content = (count == 1 ? faker.Lorem.Paragraph() : faker.Lorem.Paragraphs(count)) + Environment.NewLine;
        if (string.IsNullOrEmpty(s))
        {
            return content;
        }
        return s + Environment.NewLine + content;
    }

    internal static string AddResourceLink(this string s, string url)
    {
        var faker = new Faker();
        var content = $" ![some resource {faker.Random.Int(1)}]({url})" + Environment.NewLine;
        if (string.IsNullOrEmpty(s))
        {
            return content;
        }
        return s + Environment.NewLine + content;
    }

    internal static string AddLink(this string s, string url)
    {
        var faker = new Faker();
        var content = $" [some link {faker.Random.Int(1)}]({url})" + Environment.NewLine;
        if (string.IsNullOrEmpty(s))
        {
            return content;
        }
        return s + Environment.NewLine + content;
    }

    internal static string AddCodeLink(this string s, string name, string url)
    {
        var faker = new Faker();
        var content = $" [!code-csharp[{name}]({url})]" + Environment.NewLine;
        if (string.IsNullOrEmpty(s))
        {
            return content;
        }
        return s + Environment.NewLine + content;
    }

    internal static string AddTableStart(this string s, int columns = 3)
    {
        var faker = new Faker();
        var content = "|";
        for (var col = 0; col < columns; col++)
        {
            content += $" {faker.Lorem.Words(2)} |";
        }
        content += Environment.NewLine;
        for (var col = 0; col < columns; col++)
        {
            content += $" --- |";
        }
        content += Environment.NewLine;
        if (string.IsNullOrEmpty(s))
        {
            return content;
        }
        return s + Environment.NewLine + content;
    }

    internal static string AddTableRow(this string s, params string[] columns)
    {
        var faker = new Faker();
        var content = "|";
        foreach (var col in columns)
        {
            content += $" {col} |";
        }
        content += Environment.NewLine;
        if (string.IsNullOrEmpty(s))
        {
            return content;
        }
        return s + Environment.NewLine + content;
    }

    internal static string AddNewLine(this string s)
    {
        if (string.IsNullOrEmpty(s))
        {
            return Environment.NewLine;
        }
        return s + Environment.NewLine;
    }

    internal static string AddRaw(this string s, string markdown)
    {
        if (string.IsNullOrEmpty(s))
        {
            return markdown;
        }
        return s + markdown;
    }
}
