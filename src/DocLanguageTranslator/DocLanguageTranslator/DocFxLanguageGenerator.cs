// Licensed to DocFX Companion Tools and contributors under one or more agreements.
// DocFX Companion Tools and contributors licenses this file to you under the MIT license.
using System.Globalization;
using System.Text.RegularExpressions;
using DocFXLanguageGenerator.Domain;
using DocFXLanguageGenerator.Helpers;
using DocLanguageTranslator.FileService;
using DocLanguageTranslator.TranslationService;
using Markdig;

namespace DocFXLanguageGenerator
{
    /// <summary>
    /// DocFx language generator.
    /// </summary>
    internal class DocFxLanguageGenerator
    {
        private readonly CommandlineOptions options;
        private readonly MessageHelper message;
        private readonly MarkdownPipeline markdownPipeline;
        private readonly IFileService fileService;
        private readonly ITranslationService translationService;

        private string subscriptionKey;
        private int returnValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="DocFxLanguageGenerator"/> class.
        /// </summary>
        /// <param name="options">The command line options.</param>
        /// <param name="fileService">The file service.</param>
        /// <param name="translationService">The translation service.</param>
        public DocFxLanguageGenerator(
            CommandlineOptions options,
            IFileService fileService,
            ITranslationService translationService)
        {
            this.options = options;
            this.fileService = fileService;
            this.translationService = translationService;
            this.message = new MessageHelper(options);
            markdownPipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();
            subscriptionKey = options.Key;
        }

        /// <summary>
        /// Runs the generator.
        /// </summary>
        /// <returns><c>0</c> if successful; otherwise <c>1</c>.</returns>
        public int Run()
        {
            returnValue = 0;
            int numberOfFiles = 0;

            message.Verbose($"Documentation folder: {options.DocFolder}");
            message.Verbose($"Verbose             : {options.Verbose}");
            message.Verbose($"Check structure only: {options.CheckOnly}");
            message.Verbose($"Key                 : {options.Key}");
            message.Verbose($"Location            : {options.Location}");

            if (string.IsNullOrEmpty(subscriptionKey) && !options.CheckOnly)
            {
                message.Error("ERROR: you have to have an Azure Cognitive Service key if you are not only checking the structure.");
                return 1;
            }

            if (!fileService.DirectoryExists(options.DocFolder))
            {
                message.Error($"ERROR: Documentation folder '{options.DocFolder}' doesn't exist.");
                return 1;
            }

            // Here we take the root directory passed for example ./userdocs
            // We expect to have sub folders like ./userdocs/en ./userdocs/de, etc
            string rootDirectory = options.DocFolder;
            var allLanguagesDirectories = FindAllRootLanguages(rootDirectory);

            foreach (var langDir in allLanguagesDirectories)
            {
                // Get all the Markdown files
                var allMarkdowns = FindAllMarkdownFiles(langDir);
                string sourceLang = GetLanguageCodeFromPath(langDir);

                // checked that the file exists in other directories
                foreach (var markdown in allMarkdowns)
                {
                    foreach (var lgDir in allLanguagesDirectories)
                    {
                        if (langDir == lgDir)
                        {
                            continue;
                        }

                        string targetLang = GetLanguageCodeFromPath(lgDir);
                        var targetFile = markdown.Replace(langDir, lgDir);
                        if (!fileService.FileExists(targetFile))
                        {
                            if (options.CheckOnly)
                            {
                                message.Error($"ERROR: file {targetFile} is missing.");
                                numberOfFiles++;
                                returnValue = 1;
                            }
                            else if (TranslateMarkdown(markdown, sourceLang, targetFile, targetLang))
                            {
                                numberOfFiles++;
                            }
                        }
                    }
                }
            }

            PrintCompletionMessage(numberOfFiles);
            return returnValue;
        }

        /// <summary>
        /// Extracts the language code from the given path.
        /// </summary>
        /// <param name="path">The path to extract the language code from.</param>
        /// <returns>A string representing the language code (e.g., "en-US", "fr-FR") extracted from the path.</returns>
        internal string GetLanguageCodeFromPath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentException("Path cannot be empty", nameof(path));
            }

            // Normalize path separators
            path = path.Replace('\\', '/').TrimEnd('/');

            // Extract last directory name
            var lastSeparator = path.LastIndexOf('/');
            string dirName = lastSeparator >= 0
                ? path.Substring(lastSeparator + 1)
                : path;

            // Handle two-letter codes vs complex codes
            if (dirName.Contains('-') || dirName.Contains('_'))
            {
                // Complex language code with region/script
                return dirName;
            }

            // Simple two-letter code (convert to lowercase)
            return dirName.Length >= 2
                ? dirName.Substring(0, 2).ToLower(CultureInfo.InvariantCulture)
                : dirName;
        }

        private static string TransformMarkdown(string input, MarkdownPipeline pipeline, Func<string, string> func)
        {
            using var writer = new StringWriter();
            var renderer = new ReplacementRenderer(writer, input, func);
            renderer.Render(Markdown.Parse(input, pipeline));

            // Flush any remaining markdown content.
            renderer.Writer.Write(renderer.TakeNext(input.Length - renderer.LastWrittenIndex));
            return writer.ToString();
        }

        private void PrintCompletionMessage(int numberOfFiles)
        {
            string finalOutput = "Process finished.";
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

        private string[] FindAllMarkdownFiles(string rootDirectory)
        => fileService.GetFiles(rootDirectory, "*.md", SearchOption.AllDirectories);

        private static readonly HashSet<string> ValidCultureNames = new HashSet<string>(
            CultureInfo.GetCultures(CultureTypes.AllCultures)
                .Select(c => c.Name)
                .Where(name => !string.IsNullOrEmpty(name)),
            StringComparer.OrdinalIgnoreCase);

        private string[] FindAllRootLanguages(string rootDirectory)
        {
            return fileService.GetDirectories(rootDirectory)
                .Where(dir =>
                {
                    string dirName = new DirectoryInfo(dir).Name;
                    return ValidCultureNames.Contains(dirName);
                })
                .ToArray();
        }

        private bool TranslateMarkdown(string inputFile, string sourceLang, string outputFile, string targetLang)
        {
            try
            {
                message.Verbose($"Translating {inputFile} [{sourceLang} to {targetLang}]");
                string mdContent = fileService.ReadAllText(inputFile);

                string translated = TransformMarkdown(mdContent, markdownPipeline, value =>
                    ProcessMarkdownSegment(value, sourceLang, targetLang));

                Console.WriteLine();

                // Clean the results as when translating relative path and link on images are distorded
                translated = translated.Replace("! [", "![").Replace("] (", "](").Replace("](.. /", "](../");

                EnsureDirectoryExists(Path.GetDirectoryName(outputFile));

                // Save the file
                message.Verbose($"Saving {outputFile}");
                fileService.WriteAllText(outputFile, translated);
                return true;
            }
#pragma warning disable CA1031 // Quite a lot of exceptions can happen here
            catch (Exception ex)
#pragma warning restore CA1031 // So catching all of them rather than a long list of individual ones
            {
                message.Error($"ERROR processing {inputFile}: {ex.Message}");
                returnValue = 1;
                return false;
            }
        }

        private void EnsureDirectoryExists(string path)
        {
            if (!fileService.DirectoryExists(path))
            {
                fileService.CreateDirectory(path);
            }
        }

        private string ProcessMarkdownSegment(string value, string sourceLang, string targetLang)
        {
            Console.Write(".");
            var links = ExtractMarkdownLinks(ref value);
            string translated = translationService.TranslateAsync(value, sourceLang, targetLang).GetAwaiter().GetResult();
            return RestoreMarkdownLinks(translated, links);
        }

        private List<string> ExtractMarkdownLinks(ref string markdown)
        {
            var links = new List<string>();

            // Check if it's a relative link or URL
            var matches = Regex.Matches(markdown, @"\]\(([^)]*)");

            // Get rid of what is in the (), it's a link, store it and add it later on
            foreach (Match match in matches)
            {
                // Groups[1] always exist and is what is the url:
                // [text](Groups[1])
                links.Add(match.Groups[1].Value);

                // Even if there is a double existance, it doesn't matter, we have them in the list
                markdown = markdown.Replace(match.Groups[1].Value, string.Empty);
            }

            return links;
        }

        private string RestoreMarkdownLinks(string markdown, List<string> links)
        {
            if (links.Count == 0)
            {
                return markdown;
            }

            // We know that a space will be inserted sometimes
            string result = markdown.Replace("] (", "](").Replace("! [", "![").Replace("[! ", "[!");
            foreach (var link in links)
            {
                int pos = result.IndexOf("]()");
                if (pos != -1)
                {
                    // It happens that the parenthesis are fully removed. In this casen we will add the link at the beginning
                    result = result.Insert(pos + 2, link);
                }
            }

            return result;
        }
    }
}
