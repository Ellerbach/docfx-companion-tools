// <copyright file="FileInfoService.cs" company="DocFx Companion Tools">
// Copyright (c) DocFx Companion Tools. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
using System.Diagnostics;
using System.Text.RegularExpressions;
using Markdig;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Readers;

namespace DocFxTocGenerator.FileService;

/// <summary>
/// Helper class for file names.
/// </summary>
public class FileInfoService
{
    private readonly bool _camelCasing;
    private readonly IFileService _fileService;
    private readonly ILogger _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="FileInfoService"/> class.
    /// </summary>
    /// <param name="camelCasing">Use camel casing for titles.</param>
    /// <param name="fileService">File service.</param>
    /// <param name="logger">Logger.</param>
    public FileInfoService(bool camelCasing, IFileService fileService, ILogger logger)
    {
        _camelCasing = camelCasing;
        _fileService = fileService;
        _logger = logger;
    }

    /// <summary>
    /// Create <see cref="FileData"/> object for the given file.
    /// </summary>
    /// <param name="folder">Parent folder of the file.</param>
    /// <param name="file">File path.</param>
    /// <returns><see cref="FileData"/> object.</returns>
    public FileData CreateFileData(FolderData folder, string file)
    {
        string fname = Path.GetFileNameWithoutExtension(file);
        FileData filedata = new()
        {
            Parent = folder,
            Name = Path.GetFileName(file),
            Path = Path.Combine(folder.Path, file).NormalizePath(),
            DisplayName = ToTitleCase(Path.GetFileNameWithoutExtension(file), _camelCasing),
        };

        StringComparer comparer = StringComparer.Ordinal;
        StringComparison comparison = StringComparison.Ordinal;

        // readme and index must be handled case-insensitive to work properly
        if (fname.Equals("readme", StringComparison.OrdinalIgnoreCase) ||
            fname.Equals("index", StringComparison.OrdinalIgnoreCase))
        {
            comparer = StringComparer.OrdinalIgnoreCase;
            comparison = StringComparison.OrdinalIgnoreCase;
        }

        if (folder.OrderList.Contains(fname, comparer))
        {
            // set the order
            filedata.Sequence = folder.OrderList.FindIndex(x => x.Equals(fname, comparison));
            Debug.WriteLine($"Folder '{folder.Path}' File '{filedata.Name}' Sequence '{filedata.Sequence}'");
        }

        var title = GetFileDisplayName(file, _camelCasing);
        if (folder.OverrideList.TryGetValue(fname, out string? name))
        {
            // override the display name
            filedata.DisplayName = name;
            filedata.IsDisplayNameOverride = true;
        }
        else
        {
            filedata.DisplayName = title;
        }

        return filedata;
    }

    /// <summary>
    /// Get the title for the file. Default is the name of the file.
    /// For a markdown file we will get the first H1 from the file as title.
    /// For a OpenAPI swagger file we will get the title and version as title.
    /// </summary>
    /// <param name="filePath">Path to the file.</param>
    /// <param name="camelCase">Camel case the title (capitalize first letter).</param>
    /// <returns>A cleaned string replacing - and _ as well as non authorized characters.</returns>
    public string GetFileDisplayName(string filePath, bool camelCase)
    {
        string name = Path.GetFileNameWithoutExtension(filePath);

        if (Path.GetExtension(filePath).Equals(".md", StringComparison.OrdinalIgnoreCase))
        {
            // For markdownfile, open the file, read the line up to the first #, extract the tile
            name = GetMarkdownTitle(filePath);
        }
        else if (filePath.EndsWith("swagger.json", StringComparison.OrdinalIgnoreCase))
        {
            try
            {
                // for open api swagger file, read the title from the data.
                using var stream = _fileService.OpenRead(filePath);
                var document = new OpenApiStreamReader().Read(stream, out _);
                name = $"{document.Info.Title} {document.Info.Version}".Trim();
            }
            catch (Exception ex)
            {
                // trim ".swagger", as the first one above just trimmed ".json".
                name = Path.GetFileNameWithoutExtension(name);
                _logger.LogError($"Reading {filePath} for the title failed. Error: {ex.Message}. Using file name as display name.");
            }
        }

        return ToTitleCase(name, camelCase);
    }

    /// <summary>
    /// Uppercase first character and remove unwanted characters.
    /// </summary>
    /// <param name="title">The name to clean.</param>
    /// <param name="camelCase">Camel case the title (capitalize first letter).</param>
    /// <returns>A clean name.</returns>
    public string ToTitleCase(string title, bool camelCase)
    {
        if (string.IsNullOrEmpty(title))
        {
            return string.Empty;
        }

        // see if we have to strip a file extension.
        string cleantitle = title;
        cleantitle = Regex.Replace(cleantitle, @"([\[\]\:`\\{}()\*/])", string.Empty);
        cleantitle = Regex.Replace(cleantitle, @"[-_+\s]+", " ");
        cleantitle = camelCase ?
            string.Concat(cleantitle.First().ToString().ToLowerInvariant(), cleantitle.AsSpan(1)) :
            string.Concat(cleantitle.First().ToString().ToUpperInvariant(), cleantitle.AsSpan(1));
        return cleantitle;
    }

    /// <summary>
    /// Get the H1 title from a markdown file.
    /// </summary>
    /// <param name="filePath">File path of the markdown file.</param>
    /// <returns>First H1, or the filename as title if that fails.</returns>
    private string GetMarkdownTitle(string filePath)
    {
        string markdownFilePath = _fileService.GetFullPath(filePath);
        string markdown = _fileService.ReadAllText(markdownFilePath);

        MarkdownPipeline pipeline = new MarkdownPipelineBuilder()
        .UseAdvancedExtensions()
        .Build();
        MarkdownDocument document = Markdig.Markdown.Parse(markdown, pipeline);

        // get the first H1 header
        var h1 = document
            .Descendants<HeadingBlock>()
            .FirstOrDefault(x => x.Level == 1);

        if (h1 != null)
        {
            string title = string.Empty;

            var child = h1.Inline!.Descendants<LiteralInline>().FirstOrDefault();
            if (child != null)
            {
                title = markdown.Substring(child.Span.Start, h1.Span.Length - (child.Span.Start - h1.Span.Start));
            }
            else if (h1.Inline!.FirstChild != null)
            {
                // fallback for complex headers, like "# `text with quotes`" and such
                title = markdown.Substring(h1.Inline.FirstChild.Span.Start, h1.Span.Length - (h1.Inline.FirstChild.Span.Start - h1.Span.Start));
            }

            return title;
        }

        // in case we couldn't get an H1 from markdown, return the filepath sanitized.
        return ToTitleCase(Path.GetFileNameWithoutExtension(filePath), _camelCasing);
    }
}
