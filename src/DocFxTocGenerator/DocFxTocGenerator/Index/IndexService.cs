// <copyright file="IndexService.cs" company="DocFx Companion Tools">
// Copyright (c) DocFx Companion Tools. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
using DocFxTocGenerator.FileService;
using DocFxTocGenerator.Liquid;
using Microsoft.Extensions.Logging;

namespace DocFxTocGenerator.Index;

/// <summary>
/// Service to generate an index for a folder.
/// </summary>
public class IndexService
{
    private readonly LiquidService _liquidService;
    private readonly IFileService _fileService;
    private readonly ILogger _logger;

    private readonly string _defaultIndexTemplate =
@"# {{ current.DisplayName }}

{% comment -%}Looping through all the files and show the display name.{%- endcomment -%}
{% for file in current.Files -%}
{%- if file.IsMarkdown -%}
* [{{ file.DisplayName }}]({{ file.Name }})
{% endif -%}
{%- endfor %}
";

    /// <summary>
    /// Initializes a new instance of the <see cref="IndexService"/> class.
    /// </summary>
    /// <param name="fileService">File service.</param>
    /// <param name="logger">Logger.</param>
    public IndexService(IFileService fileService, ILogger logger)
    {
        _fileService = fileService;
        _logger = logger;
        _liquidService = new LiquidService(logger);
    }

    /// <summary>
    /// Generate an index.md for the currentFolder.
    /// </summary>
    /// <param name="rootFolder">Root folder of the content.</param>
    /// <param name="currentFolder">Current folder to generate index for.</param>
    /// <returns>Path of created index. Empty on error.</returns>
    public string GenerateIndex(FolderData rootFolder, FolderData currentFolder)
    {
        string template = GetIndexTemplate(currentFolder);

        try
        {
            string content = _liquidService.Render(rootFolder, currentFolder, template);
            string indexPath = Path.Combine(currentFolder.Path, "index.md");
            _fileService.WriteAllText(indexPath, content);
            _logger.LogInformation($"Index.md generated for `{currentFolder.Path}`.");
            return indexPath;
        }
        catch (Exception)
        {
            return string.Empty;
        }
    }

    private string GetIndexTemplate(FolderData folder)
    {
        string? path = folder.Path;

        // walk up the folder tree to find the first Liquid template file.
        while (path != null)
        {
            string filePath = Path.Combine(path, ".index.liquid");
            if (_fileService.ExistsFileOrDirectory(filePath))
            {
                // found one, so return the content
                _logger.LogInformation($"Found `{filePath}` as template for index.md in `{folder.Path}`");
                return _fileService.ReadAllText(filePath);
            }

            path = Directory.GetParent(path)?.FullName;
        }

        // couldn't find a file, so return the default template.
        return _defaultIndexTemplate;
    }
}
