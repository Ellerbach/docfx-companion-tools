// <copyright file="FileInfoService.cs" company="DocFx Companion Tools">
// Copyright (c) DocFx Companion Tools. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
using System;
using System.Diagnostics;
using Markdig;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using Microsoft.Extensions.Logging;

namespace DocAssembler.FileService;

/// <summary>
/// File info service.
/// </summary>
public class FileInfoService
{
    private readonly string _workingFolder;
    private readonly IFileService _fileService;
    private readonly ILogger _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="FileInfoService"/> class.
    /// </summary>
    /// <param name="workingFolder">Working folder.</param>
    /// <param name="fileService">File service.</param>
    /// <param name="logger">Logger.</param>
    public FileInfoService(string workingFolder, IFileService fileService, ILogger logger)
    {
        _workingFolder = workingFolder;
        _fileService = fileService;
        _logger = logger;
    }

    /// <summary>
    /// Get the local links in the markdown file.
    /// </summary>
    /// <param name="root">Root path of the documentation.</param>
    /// <param name="filePath">File path of the markdown file.</param>
    /// <returns>List of local links in the document. If none found, the list is empty.</returns>
    public List<Hyperlink> GetLocalHyperlinks(string root, string filePath)
    {
        string markdownFilePath = _fileService.GetFullPath(filePath);
        string markdown = _fileService.ReadAllText(markdownFilePath);

        MarkdownPipeline pipeline = new MarkdownPipelineBuilder()
            .UseAdvancedExtensions()
            .Build();
        MarkdownDocument document = Markdown.Parse(markdown, pipeline);

        // get all links
        var links = document
            .Descendants<LinkInline>()
            .Where(x => !x.UrlHasPointyBrackets &&
                        !string.IsNullOrEmpty(x.Url) &&
                        !x.Url.StartsWith("https://", StringComparison.InvariantCulture) &&
                        !x.Url.StartsWith("http://", StringComparison.InvariantCulture) &&
                        !x.Url.StartsWith("ftps://", StringComparison.InvariantCulture) &&
                        !x.Url.StartsWith("ftp://", StringComparison.InvariantCulture) &&
                        !x.Url.StartsWith("mailto:", StringComparison.InvariantCulture) &&
                        !x.Url.StartsWith("xref:", StringComparison.InvariantCulture))
            .Select(d => new Hyperlink(markdownFilePath, d.Line + 1, d.Column + 1, d.Url ?? string.Empty)
            {
                UrlSpanStart = d.UrlSpan.Start,
                UrlSpanEnd = d.UrlSpan.End,
                UrlSpanLength = d.UrlSpan.Length,
            })
            .ToList();

        // updating the links
        foreach (var link in links)
        {
            if (link.Url != null && !link.Url.Equals(markdown.Substring(link.UrlSpanStart, link.UrlSpanLength), StringComparison.Ordinal))
            {
                // MARKDIG FIX
                // In some cases the Url in MarkDig LinkInline is not equal to the original
                // e.g. a link "..\..\somefile.md" resolves in "....\somefile.md"
                // we fix that here. This will probably not be fixed in the markdig
                // library, as you shouldn't use backslash, but Unix-style slash.
                link.OriginalUrl = markdown.Substring(link.UrlSpanStart, link.UrlSpanLength);
                link.Url = markdown.Substring(link.UrlSpanStart, link.UrlSpanLength);
            }

            if (link.Url?.StartsWith('~') == true)
            {
                // special reference to root. We need to expand that to the root folder.
                link.Url = _workingFolder + link.Url.AsSpan(1).ToString();
            }

            if (link.IsLocal)
            {
                int pos = link.Url!.IndexOf('#', StringComparison.InvariantCulture);
                if (pos == -1)
                {
                    // if we don't have a header delimiter, we might have a url delimiter
                    pos = link.Url.IndexOf('?', StringComparison.InvariantCulture);
                }

                // we want to know that the link is not starting with a # for local reference.
                // if local reference, return the filename otherwise the calculated path.
                if (pos != 0)
                {
                    link.UrlFullPath = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(link.FilePath)!.NormalizePath(), link.UrlWithoutTopic)).NormalizePath();
                }
            }
            else
            {
                link.UrlFullPath = link.Url!;
            }
        }

        return links;
    }
}
