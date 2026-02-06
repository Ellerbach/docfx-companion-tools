// Licensed to DocFX Companion Tools and contributors under one or more agreements.
// DocFX Companion Tools and contributors licenses this file to you under the MIT license.

using DocLanguageTranslator.FileService;

namespace DocLanguageTranslator.TranslationMode;

/// <summary>
/// Translation mode for translating an entire file and writing to a new output file.
/// </summary>
internal class FullFileTranslationMode : ITranslationMode
{
    private readonly Action<string> ensureDirectoryExists;

    /// <summary>
    /// Initializes a new instance of the <see cref="FullFileTranslationMode"/> class.
    /// </summary>
    /// <param name="ensureDirectoryExists">Action to ensure the output directory exists.</param>
    public FullFileTranslationMode(Action<string> ensureDirectoryExists)
    {
        this.ensureDirectoryExists = ensureDirectoryExists;
    }

    /// <inheritdoc/>
    public string ReadContent(IFileService fileService, string inputFile)
        => fileService.ReadAllText(inputFile);

    /// <inheritdoc/>
    public void WriteContent(IFileService fileService, string outputFile, string translatedContent)
    {
        var directory = Path.GetDirectoryName(outputFile);
        if (directory is not null)
        {
            ensureDirectoryExists(directory);
        }

        fileService.WriteAllText(outputFile, translatedContent);
    }

    /// <inheritdoc/>
    public string FormatStartMessage(string inputFile, string outputFile, string sourceLang, string targetLang)
        => $"Translating {inputFile} [{sourceLang} to {targetLang}]";

    /// <inheritdoc/>
    public string FormatCompletionMessage(string outputFile)
        => $"Saving {outputFile}";

    /// <inheritdoc/>
    public string GetNoContentErrorMessage() => null;
}
