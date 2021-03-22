// Licensed to DocFX Companion Tools and contributors under one or more agreements.
// DocFX Companion Tools and contributors licenses this file to you under the MIT license.

namespace DocFXLanguageGenerator
{
    using Newtonsoft.Json;

    /// <summary>
    /// Error response.
    /// </summary>
    public class ErrorResponse
    {
        /// <summary>
        /// Gets or sets the error.
        /// </summary>
        [JsonProperty(PropertyName = "error")]
        public ErrorDetail Error { get; set; }
    }
}
