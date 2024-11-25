// <copyright file="FileInfoService.cs" company="DocFx Companion Tools">
// Copyright (c) DocFx Companion Tools. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
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
                        x.Url != null &&
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

        return links;
    }
}
