// Licensed to DocFX Companion Tools and contributors under one or more agreements.
// DocFX Companion Tools and contributors licenses this file to you under the MIT license.
using Newtonsoft.Json;

namespace DocFXLanguageGenerator.Domain
{
    /// <summary>
    /// Error details.
    /// </summary>
    public class ErrorDetail
    {
        /// <summary>
        /// Gets or sets the error code.
        /// </summary>
        [JsonProperty(PropertyName = "code")]
        public int Code { get; set; }

        /// <summary>
        /// Gets or sets the error message.
        /// </summary>
        [JsonProperty(PropertyName = "message")]
        public string Message { get; set; }
    }
}
