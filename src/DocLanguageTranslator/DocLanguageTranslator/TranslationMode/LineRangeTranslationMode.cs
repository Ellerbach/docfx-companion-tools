// Licensed to DocFX Companion Tools and contributors under one or more agreements.
// DocFX Companion Tools and contributors licenses this file to you under the MIT license.

using DocLanguageTranslator.FileService;

namespace DocLanguageTranslator.TranslationMode;

/// <summary>
/// Translation mode for translating a specific range of lines and replacing them in an existing file.
/// </summary>
internal class LineRangeTranslationMode : ITranslationMode
{
    private readonly int startLine;
    private readonly int endLine;

    /// <summary>
    /// Initializes a new instance of the <see cref="LineRangeTranslationMode"/> class.
    /// </summary>
    /// <param name="startLine">The 1-based starting line number.</param>
    /// <param name="endLine">The 1-based ending line number (inclusive).</param>
    public LineRangeTranslationMode(int startLine, int endLine)
    {
        this.startLine = startLine;
        this.endLine = endLine;
    }

    /// <inheritdoc/>
    public string ReadContent(IFileService fileService, string inputFile)
    {
        string[] sourceLines = fileService.ReadLines(inputFile, startLine, endLine);
        return sourceLines.Length == 0 ? null : string.Join(Environment.NewLine, sourceLines);
    }

    /// <inheritdoc/>
    public void WriteContent(IFileService fileService, string outputFile, string translatedContent)
    {
        string[] translatedLines = translatedContent.Split(["\r\n", "\r", "\n"], StringSplitOptions.None);
        fileService.ReplaceLines(outputFile, startLine, endLine, translatedLines);
    }

    /// <inheritdoc/>
    public string FormatStartMessage(string inputFile, string outputFile, string sourceLang, string targetLang)
        => $"Translating lines {startLine}-{endLine} from {inputFile} to {outputFile} [{sourceLang} to {targetLang}]";

    /// <inheritdoc/>
    public string FormatCompletionMessage(string outputFile)
        => $"Updated lines {startLine}-{endLine} in {outputFile}";

    /// <inheritdoc/>
    public string GetNoContentErrorMessage()
        => $"ERROR: No lines found in range {startLine}-{endLine}.";
}
