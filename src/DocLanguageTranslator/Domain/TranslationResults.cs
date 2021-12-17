// Licensed to DocFX Companion Tools and contributors under one or more agreements.
// DocFX Companion Tools and contributors licenses this file to you under the MIT license.

namespace DocFXLanguageGenerator.Domain
{
    using Newtonsoft.Json;

    /// <summary>
    /// The translation result from the call to Azure Cognitive Service API.
    /// </summary>
    public class TranslationResults
    {
        /// <summary>
        /// Gets or sets the translation array.
        /// </summary>
        [JsonProperty(PropertyName = "translations")]
        public Translation[] Translations { get; set; }
    }
}
