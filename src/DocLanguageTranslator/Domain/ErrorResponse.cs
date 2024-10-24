﻿// Licensed to DocFX Companion Tools and contributors under one or more agreements.
// DocFX Companion Tools and contributors licenses this file to you under the MIT license.
using Newtonsoft.Json;

namespace DocFXLanguageGenerator.Domain
{
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
