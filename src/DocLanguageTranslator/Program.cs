// Licensed to DocFX Companion Tools and contributors under one or more agreements.
// DocFX Companion Tools and contributors licenses this file to you under the MIT license.

namespace DocFXLanguageGenerator
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;
    using CommandLine;
    using DocFXLanguageGenerator.Helpers;
    using Markdig;
    using Newtonsoft.Json;

    /// <summary>
    /// The core program.
    /// </summary>
    internal class Program
    {
        private const string Endpoint = "https://api.cognitive.microsofttranslator.com/";
        private const string DefaultLocation = "westeurope";
        private static string location;
        private static string subscriptionKey;
        private static CommandlineOptions options;
        private static MessageHelper message;
        private static int returnvalue;
        private static MarkdownPipeline markdownPipeline;

        private static int Main(string[] args)
        {
            markdownPipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();

            Parser.Default.ParseArguments<CommandlineOptions>(args)
                   .WithParsed<CommandlineOptions>(RunLogic)
                   .WithNotParsed(HandleErrors);

            Console.WriteLine($"Exit with return code {returnvalue}");

            return returnvalue;
        }

        private static void RunLogic(CommandlineOptions o)
        {
            int numberOfFiles = 0;
            options = o;
            message = new MessageHelper(options);

            message.Verbose($"Documentation folder: {options.DocFolder}");
            message.Verbose($"Verbose             : {options.Verbose}");
            message.Verbose($"Check structure only: {options.CheckOnly}");
            message.Verbose($"Key                 : {options.Key}");
            message.Verbose($"Location            : {options.Location}");

            if (string.IsNullOrEmpty(options.Key) && !options.CheckOnly)
            {
                message.Error($"ERROR: you have to have an Azure Cognitive Service key if you are not only checking the structure.");
                returnvalue = 1;
                return;
            }

            subscriptionKey = options.Key;
            location = string.IsNullOrEmpty(options.Location) ? DefaultLocation : options.Location;

            if (!Directory.Exists(options.DocFolder))
            {
                message.Error($"ERROR: Documentation folder '{options.DocFolder}' doesn't exist.");
                returnvalue = 1;
                return;
            }

            // Here we take the root directory passed for example ./userdocs
            // We expect to have sub folders like ./userdocs/en ./userdocs/de, etc
            string rootDirectopry = options.DocFolder;
            var allLanguagesDirectories = FindAllRootLangauges(rootDirectopry);
            foreach (var langDir in allLanguagesDirectories)
            {
                // Get all the Markdown files
                var allMarkdowns = FindAllMarkdownFiles(langDir);
                // checked that the file exists in other directories
                foreach (var markdown in allMarkdowns)
                {
                    foreach (var lgDir in allLanguagesDirectories)
                    {
                        if (langDir == lgDir)
                        {
                            continue;
                        }

                        var filName = markdown.Replace(langDir, lgDir);
                        if (!File.Exists(filName))
                        {
                            if (options.CheckOnly)
                            {
                                message.Error($"ERROR: file {filName} is missing.");
                                numberOfFiles++;
                                returnvalue = 1;
                            }
                            else
                            {
#pragma warning disable CA1308 // The langauge has to be lowercase
                                TranslateMarkdown(markdown, langDir.Substring(langDir.Length - 2).ToLower(CultureInfo.InvariantCulture),
                                    filName, lgDir.Substring(lgDir.Length - 2).ToLower(CultureInfo.InvariantCulture));
#pragma warning restore CA1308 // The langauge has to be lowercase
                                numberOfFiles++;
                            }
                        }
                    }
                }
            }

            string finalOutput = $"Process finished.";
            if (options.CheckOnly && numberOfFiles > 0)
            {
                finalOutput += $" {numberOfFiles} missing. Please check the previous lines and create them or adjust those existing.";
            }
            else if (numberOfFiles > 0)
            {
                finalOutput += $" {numberOfFiles} translated and properly created. Please make sure to run the Markdown linter and also check the file links and images.";
            }

            Console.WriteLine(finalOutput);
        }

        /// <summary>
        /// On parameter errors, we set the returnvalue to 1 to indicated an error.
        /// </summary>
        /// <param name="errors">List or errors (ignored).</param>
        private static void HandleErrors(IEnumerable<CommandLine.Error> errors)
        {
            returnvalue = 1;
        }

        private static string[] FindAllMarkdownFiles(string rootDirectory)
        {
            return Directory.GetFiles(rootDirectory, "*.md", SearchOption.AllDirectories);
        }

        private static string[] FindAllRootLangauges(string rootDirectory)
        {
            List<string> dirLanguages = new List<string>();
            var dirs = Directory.GetDirectories(rootDirectory);
            foreach (var dir in dirs)
            {
                var dirInfo = new DirectoryInfo(dir);
                if (dirInfo.Name.Length == 2)
                {
                    dirLanguages.Add(dir);
                }
            }

            return dirLanguages.ToArray();
        }

        private static void TranslateMarkdown(string inputFile, string inputLanguage, string outputFile, string outputLanguage)
        {

            // TODO: detect the language from and to from the URL with the /de /fr, etc conventions
            Console.WriteLine($"Translating {inputFile}");
            Console.WriteLine($"Translating from {inputLanguage} to {outputLanguage}");
            using StreamReader sr = new StreamReader(inputFile);
            string mdFileContent = sr.ReadToEnd();
            sr.Close();
            sr.Dispose();
            string translatedMarkdown = string.Empty;
            try
            {
                translatedMarkdown = TransformMarkdown(mdFileContent, markdownPipeline, value =>
                {
                    Console.Write(".");

                    // Translate
                    var res = Translate(value, inputLanguage, outputLanguage).GetAwaiter().GetResult();
                    return res;
                });
            }
#pragma warning disable CA1031 // Quite a lot of exceptions can happen here
            catch (Exception ex)
#pragma warning restore CA1031 // So catching all of them rather than a long list of individual ones
            {
                Console.WriteLine();
                message.Error($"ERROR: Exception {ex}");
                returnvalue = 1;
            }

            Console.WriteLine();
            // Clean the results as when translating relative path and link on images are distorded
            translatedMarkdown = translatedMarkdown.Replace("! [", "![").Replace("] (", "](").Replace("](.. /", "](../");
            // Save the file
            message.Verbose($"Saving {outputFile}");
            if (!Directory.Exists(Path.GetDirectoryName(outputFile)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(outputFile));
            }

            using StreamWriter sw = new StreamWriter(outputFile);
            sw.Write(translatedMarkdown);
            sw.Close();
            sw.Dispose();
        }

        private static async Task<string> Translate(string textToTranslate, string fromLanguage, string toLanguage)
        {
            const int WaitTime = 20;
            const int MaxRetry = 3;
            int retry = 0;
            string route = $"/translate?api-version=3.0&from={fromLanguage}&to={toLanguage}";
            object[] body = new object[] { new { Text = textToTranslate } };
            var requestBody = JsonConvert.SerializeObject(body);

            using (var client = new HttpClient())
            using (var request = new HttpRequestMessage())
            {
                // Build the request.
                request.Method = HttpMethod.Post;
                request.RequestUri = new Uri(Endpoint + route);
                request.Content = new StringContent(requestBody, Encoding.UTF8, "application/json");
                request.Headers.Add("Ocp-Apim-Subscription-Key", subscriptionKey);
                request.Headers.Add("Ocp-Apim-Subscription-Region", location);
            retry:

                // Send the request and get response.
                HttpResponseMessage response = await client.SendAsync(request).ConfigureAwait(false);
                string result = await response.Content.ReadAsStringAsync();
                try
                {
                    TranslationResults[] res = JsonConvert.DeserializeObject<TranslationResults[]>(result);
                    return res[0].Translations[0].Text;
                }
                catch
                {
                    try
                    {
                        // If it an error?
                        ErrorResponse res = JsonConvert.DeserializeObject<ErrorResponse>(result);

                        // Which error is it?
                        // 429000, 429001, 429002  The server rejected the request because the client has exceeded request limits.
                        // 500000 An unexpected error occurred. If the error persists, report it with date / time of error,
                        // request identifier from response header X - RequestId, and client identifier from request header X - ClientTraceId.
                        // 503000 Service is temporarily unavailable
                        if ((res.Error.Code == 429000) || (res.Error.Code == 429001) ||
                            (res.Error.Code == 429002) || (res.Error.Code == 500000) ||
                            (res.Error.Code == 503000))
                        {
                            if (retry < MaxRetry)
                            {
                                retry++;
                                message.Warning($"An error occured which require to wait and retry later. Waiting for {WaitTime} seconds. Retry {retry}/{MaxRetry}.");
                                goto retry;
                            }
                            else
                            {
                                message.Error($"ERROR: maximum number of rety reached, please give some time, and try again later.");
                                returnvalue = 1;
                            }
                        }

                        throw new Exception($"Exception during translation process: {res.Error.Message}");

                    }
                    catch (Exception)
                    {
                        throw;
                    }
                }
            }
        }

        private static string TransformMarkdown(string input, MarkdownPipeline pipeline, Func<string, string> func)
        {
            using (var writer = new StringWriter())
            {
                var renderer = new ReplacementRenderer(writer, input, func);
                var document = Markdown.Parse(input, pipeline);
                renderer.Render(document);

                // Flush any remaining markdown content.
                renderer.Writer.Write(renderer.TakeNext(renderer.OriginalMarkdown.Length - renderer.LastWrittenIndex));
                return writer.ToString();
            }
        }
    }
}
