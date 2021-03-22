// Licensed to DocFX Companion Tools and contributors under one or more agreements.
// DocFX Companion Tools and contributors licenses this file to you under the MIT license.

namespace DocFXLanguageGenerator
{
    using Newtonsoft.Json;

    /// <summary>
    /// Actual translation results.
    /// </summary>
    public class Translation
    {
        /// <summary>
        /// Gets or sets the translated text.
        /// </summary>
        [JsonProperty(PropertyName = "text")]
        public string Text { get; set; }

        /// <summary>
        /// Gets or sets the translated language.
        /// </summary>
        [JsonProperty(PropertyName = "to")]
        public string To { get; set; }
    }
}
