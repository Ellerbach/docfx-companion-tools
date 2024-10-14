// <copyright file="LiquidService.cs" company="DocFx Companion Tools">
// Copyright (c) DocFx Companion Tools. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
using DocFxTocGenerator.FileService;
using Fluid;
using Microsoft.Extensions.Logging;

namespace DocFxTocGenerator.Liquid;

/// <summary>
/// The service to process a liquid template with the provided content.
/// </summary>
public class LiquidService
{
    private readonly ILogger _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="LiquidService"/> class.
    /// </summary>
    /// <param name="logger">Logger.</param>
    public LiquidService(ILogger logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Renders a string as a template.
    /// </summary>
    /// <param name="rootFolder">Root folder of the content.</param>
    /// <param name="currentFolder">Current folder to render file for.</param>
    /// <param name="templateContent">The template content.</param>
    /// <returns>A rendered template.</returns>
    public string Render(
        FolderData rootFolder,
        FolderData currentFolder,
        string templateContent)
    {
        var parser = new FluidParser();

        if (string.IsNullOrEmpty(templateContent))
        {
            _logger.LogWarning($"No content for Liquid Template to parse. Returning empty string.");
            return string.Empty;
        }

        // validating the template first
        if (parser.TryParse(templateContent, out IFluidTemplate template, out string error))
        {
            // now do the actual parsing
            TemplateOptions options = new TemplateOptions();
            options.MemberAccessStrategy = new UnsafeMemberAccessStrategy();

            var ctx = new TemplateContext(new { }, options, true);

            ctx.SetValue("current", currentFolder);
            ctx.SetValue("root", rootFolder);

            try
            {
                return template.Render(ctx);
            }
            catch (Exception ex)
            {
                _logger.LogCritical($"Failed to render Liquid template: {ex.Message}");
                throw new LiquidException(ex.Message, ex);
            }
        }
        else
        {
            _logger.LogCritical($"Error in parsing the Liquid template: {error}");
            throw new LiquidException($"Parse error: {error}");
        }
    }
}
