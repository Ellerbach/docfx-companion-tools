// Licensed to DocFX Companion Tools and contributors under one or more agreements.
// DocFX Companion Tools and contributors licenses this file to you under the MIT license.

namespace DocLanguageTranslator.TranslationService;

/// <summary>
/// Provides translation services between different languages.
/// </summary>
public interface ITranslationService
{
    /// <summary>
    /// Translates text from one language to another asynchronously.
    /// </summary>
    /// <param name="text">The text to be translated.</param>
    /// <param name="sourceLang">The language code of the source text (e.g., "en" for English).</param>
    /// <param name="targetLang">The language code to translate the text into (e.g., "fr" for French).</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the translated text.</returns>
    Task<string> TranslateAsync(string text, string sourceLang, string targetLang);
}
