﻿// <copyright file="Hyperlink.cs" company="DocFx Companion Tools">
// Copyright (c) DocFx Companion Tools. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
using System.Text;

namespace DocAssembler.FileService;

/// <summary>
/// Hyperlink in document.
/// </summary>
public class Hyperlink
{
    private static readonly char[] UriFragmentOrQueryString = new char[] { '#', '?' };
    private static readonly char[] AdditionalInvalidChars = @"\/?:*".ToArray();
    private static readonly char[] InvalidPathChars = Path.GetInvalidPathChars().Concat(AdditionalInvalidChars).ToArray();

    /// <summary>
    /// Initializes a new instance of the <see cref="Hyperlink"/> class.
    /// </summary>
    public Hyperlink()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Hyperlink"/> class.
    /// </summary>
    /// <param name="filePath">Path of the markdown file.</param>
    /// <param name="line">Line number.</param>
    /// <param name="col">Column.</param>
    /// <param name="url">Url.</param>
    public Hyperlink(string filePath, int line, int col, string url)
    {
        FilePath = filePath;
        Line = line;
        Column = col;

        Url = url;
        OriginalUrl = Url;

        LinkType = HyperlinkType.Empty;
        if (!string.IsNullOrWhiteSpace(url))
        {
            if (url.StartsWith("https://", StringComparison.InvariantCulture) || url.StartsWith("http://", StringComparison.InvariantCulture))
            {
                LinkType = HyperlinkType.Webpage;
            }
            else if (url.StartsWith("ftps://", StringComparison.InvariantCulture) || url.StartsWith("ftp://", StringComparison.InvariantCulture))
            {
                LinkType = HyperlinkType.Ftp;
            }
            else if (url.StartsWith("mailto:", StringComparison.InvariantCulture))
            {
                LinkType = HyperlinkType.Mail;
            }
            else if (url.StartsWith("xref:", StringComparison.InvariantCulture))
            {
                LinkType = HyperlinkType.CrossReference;
            }
            else
            {
                Url = UrlDecode(Url);

                if (Path.GetExtension(url).Equals(".md", StringComparison.OrdinalIgnoreCase) || Path.GetExtension(url) == string.Empty)
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
    /// Gets or sets the file path name of the markdown file.
    /// </summary>
    public string FilePath { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the line number in the file.
    /// </summary>
    public int Line { get; set; }

    /// <summary>
    /// Gets or sets the column in the file.
    /// </summary>
    public int Column { get; set; }

    /// <summary>
    /// Gets or sets the URL span start.
    /// </summary>
    public int UrlSpanStart { get; set; }

    /// <summary>
    /// Gets or sets the URL span end. This might differ from Markdig span end,
    /// as we're trimming any #-reference at the end.
    /// </summary>
    public int UrlSpanEnd { get; set; }

    /// <summary>
    /// Gets or sets the URL span length. This might differ from Markdig span length,
    /// as we're trimming any #-reference at the end.
    /// </summary>
    public int UrlSpanLength { get; set; }

    /// <summary>
    /// Gets or sets the URL.
    /// </summary>
    public string Url { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the original URL as found in the Markdown document. Used for reporting to user so they can find the correct location. Url will be modified.
    /// </summary>
    public string OriginalUrl { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the full destination url.
    /// </summary>
    public string? DestinationFullUrl { get; set; }

    /// <summary>
    /// Gets or sets the relative destination url.
    /// </summary>
    public string? DestinationRelativeUrl { get; set; }

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
    /// Gets the topic in the url. This is the id after the # in a local link.
    /// Otherwise it's returned empty.
    /// </summary>
    public string UrlTopic
    {
        get
        {
            if (IsLocal)
            {
                int pos = Url.IndexOf('#', StringComparison.InvariantCulture);
                if (pos == -1)
                {
                    // if we don't have a header delimiter, we might have a url delimiter
                    pos = Url.IndexOf('?', StringComparison.InvariantCulture);
                }

                // include the separator
                return pos == -1 ? string.Empty : Url.Substring(pos);
            }

            return string.Empty;
        }
    }

    /// <summary>
    /// Gets the url without a (possible) topic. This is the id after the # in a local link.
    /// </summary>
    public string UrlWithoutTopic
    {
        get
        {
            if (IsLocal)
            {
                int pos = Url.IndexOf('#', StringComparison.InvariantCulture);
                if (pos == -1)
                {
                    // if we don't have a header delimiter, we might have a url delimiter
                    pos = Url.IndexOf('?', StringComparison.InvariantCulture);
                }

                switch (pos)
                {
                    case -1:
                        return Url;
                    case 0:
                        return FilePath;
                    default:
                        return Url.Substring(0, pos);
                }
            }

            return Url;
        }
    }

    /// <summary>
    /// Gets the full path of the URL when it's a local reference, but without the topic.
    /// It's calculated relative to the file path.
    /// </summary>
    public string UrlFullPath
    {
        get
        {
            if (IsLocal)
            {
                int pos = Url.IndexOf('#', StringComparison.InvariantCulture);
                if (pos == -1)
                {
                    // if we don't have a header delimiter, we might have a url delimiter
                    pos = Url.IndexOf('?', StringComparison.InvariantCulture);
                }

                // we want to know that the link is not starting with a # for local reference.
                // if local reference, return the filename otherwise the calculated path.
                string destFullPath = pos != 0 ?
                    Path.Combine(Path.GetDirectoryName(FilePath)!, UrlWithoutTopic) :
                    FilePath;
                return Path.GetFullPath(destFullPath).NormalizePath();
            }

            return Url;
        }
    }

    /// <summary>
    /// Decoding of local Urls. Similar to logic from DocFx RelativePath class.
    /// https://github.com/dotnet/docfx/blob/cca05f505e30c5ede36973c4b989fce711f2e8ad/src/Docfx.Common/Path/RelativePath.cs .
    /// </summary>
    /// <param name="url">Url.</param>
    /// <returns>Decoded Url.</returns>
    private string UrlDecode(string url)
    {
        // This logic only applies to relative paths.
        if (Path.IsPathRooted(url))
        {
            return url;
        }

        var anchor = string.Empty;
        var index = url.IndexOfAny(UriFragmentOrQueryString);
        if (index != -1)
        {
            anchor = url.Substring(index);
            url = url.Remove(index);
        }

        var parts = url.Split('/', '\\');
        var newUrl = new StringBuilder();
        for (int i = 0; i < parts.Length; i++)
        {
            if (i > 0)
            {
                newUrl.Append('/');
            }

            var origin = parts[i];
            var value = Uri.UnescapeDataString(origin);

            var splittedOnInvalidChars = value.Split(InvalidPathChars);
            var originIndex = 0;
            var valueIndex = 0;
            for (int j = 0; j < splittedOnInvalidChars.Length; j++)
            {
                if (j > 0)
                {
                    var invalidChar = value[valueIndex];
                    valueIndex++;
                    newUrl.Append(Uri.EscapeDataString(invalidChar.ToString()));
                }

                var splitOnInvalidChars = splittedOnInvalidChars[j];
                originIndex += splitOnInvalidChars.Length;
                valueIndex += splitOnInvalidChars.Length;
                newUrl.Append(splitOnInvalidChars);
            }
        }

        newUrl.Append(anchor);
        return newUrl.ToString();
    }
}