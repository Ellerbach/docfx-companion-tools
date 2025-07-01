// Licensed to DocFX Companion Tools and contributors under one or more agreements.
// DocFX Companion Tools and contributors licenses this file to you under the MIT license.

using System.Globalization;
using System.Text;
using DocFXLanguageGenerator.Domain;
using DocLanguageTranslator.Domain;
using Newtonsoft.Json;

namespace DocLanguageTranslator.TranslationService;

/// <summary>
/// Service responsible for handling translation operations in the application.
/// </summary>
internal class TranslationService : ITranslationService
{
    private const string Endpoint = "https://api.cognitive.microsofttranslator.com/";
    private const string DefaultLocation = "westeurope";

    private readonly string subscriptionKey;
    private readonly string location;
    private readonly int maxRetries;
    private readonly int retryDelayMs;

    /// <summary>
    /// Initializes a new instance of the <see cref="TranslationService"/> class.
    /// </summary>
    /// <param name="key">The Azure Translator API subscription key required for authentication. Cannot be null.</param>
    /// <param name="region">The Azure region for the Translator service. If null or empty, defaults to the value specified in DefaultLocation.</param>
    /// <param name="maxRetries">The maximum number of retry attempts for failed API calls. Defaults to 3.</param>
    /// <param name="retryDelayMs">The delay in milliseconds between retry attempts. Defaults to 20000 (20 seconds).</param>
    /// <exception cref="ArgumentNullException">Thrown when the subscription key is null.</exception>
    public TranslationService(string key, string region = null, int maxRetries = 3, int retryDelayMs = 20000)
    {
        subscriptionKey = key ?? throw new ArgumentNullException(nameof(key));
        location = string.IsNullOrEmpty(region) ? DefaultLocation : region;
        this.maxRetries = maxRetries;
        this.retryDelayMs = retryDelayMs;
    }

    /// <inheritdoc/>
    public async Task<string> TranslateAsync(string text, string sourceLang, string targetLang)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return text;
        }

        // Normalize language codes for API request
        sourceLang = NormalizeLanguageCode(sourceLang);
        targetLang = NormalizeLanguageCode(targetLang);

        int retryCount = 0;
        string route = $"/translate?api-version=3.0&from={sourceLang}&to={targetLang}";
        var requestBody = JsonConvert.SerializeObject(new[] { new { Text = text } });

        using var client = new HttpClient();

        while (true)
        {
            // Build the request.
            using var request = new HttpRequestMessage();
            request.Method = HttpMethod.Post;
            request.RequestUri = new Uri(Endpoint + route);
            request.Content = new StringContent(requestBody, Encoding.UTF8, "application/json");
            request.Headers.Add("Ocp-Apim-Subscription-Key", subscriptionKey);
            request.Headers.Add("Ocp-Apim-Subscription-Region", location);

            // Send the request and get response.
            var response = await client.SendAsync(request);
            string result = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                return ParseTranslationResponse(result);
            }

            var error = JsonConvert.DeserializeObject<ErrorResponse>(result);
            if (!IsRetryableError(error.Error.Code) || retryCount >= maxRetries)
            {
                throw new TranslationException($"Translation failed: {error.Error.Message}", error.Error.Code);
            }

            retryCount++;
            await Task.Delay(retryDelayMs);
        }
    }

    private static string NormalizeLanguageCode(string code)
    {
        // Convert to standard format: lowercase for primary tag, title case for subtags
        if (!code.Contains('-'))
        {
            return code.ToLower();
        }

        var parts = code.Split('-');
        return $"{parts[0].ToLower()}-{CultureInfo.InvariantCulture.TextInfo.ToTitleCase(parts[1])}";
    }

    private static string ParseTranslationResponse(string jsonResponse)
    {
        var translations = JsonConvert.DeserializeObject<TranslationResults[]>(jsonResponse);
        return translations[0].Translations[0].Text;
    }

    // 429000, 429001, 429002  The server rejected the request because the client has exceeded request limits.
    // 500000 An unexpected error occurred. If the error persists, report it with date / time of error,
    // request identifier from response header X - RequestId, and client identifier from request header X - ClientTraceId.
    // 503000 Service is temporarily unavailable
    private static bool IsRetryableError(int errorCode) =>
        errorCode is 429000 or 429001 or 429002 or 500000 or 503000;
}
