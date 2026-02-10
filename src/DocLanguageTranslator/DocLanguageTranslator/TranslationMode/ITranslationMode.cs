// Licensed to DocFX Companion Tools and contributors under one or more agreements.
// DocFX Companion Tools and contributors licenses this file to you under the MIT license.

using DocLanguageTranslator.FileService;

namespace DocLanguageTranslator.TranslationMode;

/// <summary>
/// Defines the strategy for reading source content and writing translated content.
/// </summary>
internal interface ITranslationMode
{
    /// <summary>
    /// Reads the content to be translated from the source file.
    /// </summary>
    /// <param name="fileService">The file service.</param>
    /// <param name="inputFile">The input file path.</param>
    /// <returns>The content to translate, or null if reading failed.</returns>
    string ReadContent(IFileService fileService, string inputFile);

    /// <summary>
    /// Writes the translated content to the output file.
    /// </summary>
    /// <param name="fileService">The file service.</param>
    /// <param name="outputFile">The output file path.</param>
    /// <param name="translatedContent">The translated content to write.</param>
    void WriteContent(IFileService fileService, string outputFile, string translatedContent);

    /// <summary>
    /// Gets a descriptive message for verbose logging when starting translation.
    /// </summary>
    /// <param name="inputFile">The input file path.</param>
    /// <param name="outputFile">The output file path.</param>
    /// <param name="sourceLang">The source language.</param>
    /// <param name="targetLang">The target language.</param>
    /// <returns>A formatted message describing the translation operation.</returns>
    string FormatStartMessage(string inputFile, string outputFile, string sourceLang, string targetLang);

    /// <summary>
    /// Gets a descriptive message for verbose logging when translation is complete.
    /// </summary>
    /// <param name="outputFile">The output file path.</param>
    /// <returns>A formatted completion message.</returns>
    string FormatCompletionMessage(string outputFile);

    /// <summary>
    /// Gets the error message to display when no content is found to translate.
    /// </summary>
    /// <returns>An error message, or null if no specific error message is needed.</returns>
    string GetNoContentErrorMessage();
}
