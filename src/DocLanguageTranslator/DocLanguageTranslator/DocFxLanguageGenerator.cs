// Licensed to DocFX Companion Tools and contributors under one or more agreements.
// DocFX Companion Tools and contributors licenses this file to you under the MIT license.
using System.Globalization;
using System.Text.RegularExpressions;
using DocFXLanguageGenerator.Domain;
using DocFXLanguageGenerator.Helpers;
using DocLanguageTranslator.FileService;
using DocLanguageTranslator.TranslationMode;
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
        private readonly IMessageHelper messageHelper;
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
        /// <param name="messageHelper">The message helper.</param>
        public DocFxLanguageGenerator(
            CommandlineOptions options,
            IFileService fileService,
            ITranslationService translationService,
            IMessageHelper messageHelper)
        {
            this.options = options;
            this.fileService = fileService;
            this.translationService = translationService;
            this.messageHelper = messageHelper;
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

            messageHelper.Verbose($"Documentation folder: {options.DocFolder}");
            messageHelper.Verbose($"Verbose             : {options.Verbose}");
            messageHelper.Verbose($"Check structure only: {options.CheckOnly}");
            messageHelper.Verbose($"Key                 : {options.Key}");
            messageHelper.Verbose($"Location            : {options.Location}");
            messageHelper.Verbose($"Source language     : {options.SourceLanguage}");
            messageHelper.Verbose($"Source file         : {options.SourceFile}");
            messageHelper.Verbose($"Line range          : {options.LineRange}");

            if (string.IsNullOrEmpty(subscriptionKey) && !options.CheckOnly)
            {
                messageHelper.Error("ERROR: you have to have an Azure Cognitive Service key if you are not only checking the structure.");
                return 1;
            }

            if (!fileService.DirectoryExists(options.DocFolder))
            {
                messageHelper.Error($"ERROR: Documentation folder '{options.DocFolder}' doesn't exist.");
                return 1;
            }

            // Check if line range mode is enabled
            if (!string.IsNullOrEmpty(options.LineRange))
            {
                return RunLineRangeTranslation();
            }

            // Here we take the root directory passed for example ./userdocs
            // We expect to have sub folders like ./userdocs/en ./userdocs/de, etc
            string rootDirectory = options.DocFolder;
            var allLanguagesDirectories = FindAllRootLanguages(rootDirectory);

            // use the source language if it is specified
            var sourceLanguageDirectories = options.SourceLanguage != null
                ? [$"{rootDirectory}/{options.SourceLanguage}"]
                : allLanguagesDirectories;

            foreach (var langDir in sourceLanguageDirectories)
            {
                // Get all translatable files
                var allTranslatableFiles = FindAllTranslatableFiles(langDir);
                string sourceLang = GetLanguageCodeFromPath(langDir);

                // checked that the file exists in other directories
                foreach (var file in allTranslatableFiles)
                {
                    foreach (var lgDir in allLanguagesDirectories)
                    {
                        if (langDir == lgDir)
                        {
                            continue;
                        }

                        string targetLang = GetLanguageCodeFromPath(lgDir);
                        var targetFile = file.Replace(langDir, lgDir);
                        if (!fileService.FileExists(targetFile))
                        {
                            if (options.CheckOnly)
                            {
                                messageHelper.Error($"ERROR: file {targetFile} is missing.");
                                numberOfFiles++;
                                returnValue = 1;
                            }
                            else
                            {
                                var mode = new FullFileTranslationMode(EnsureDirectoryExists);
                                if (TranslateFile(file, sourceLang, targetFile, targetLang, mode))
                                {
                                    numberOfFiles++;
                                }
                            }
                        }
                    }
                }
            }

            PrintCompletionMessage(numberOfFiles);
            return returnValue;
        }

        /// <summary>
        /// Determines whether the given file path refers to a Markdown file.
        /// </summary>
        /// <param name="filePath">The file path to check.</param>
        /// <returns><c>true</c> if the file has a <c>.md</c> extension; otherwise, <c>false</c>.</returns>
        internal static bool IsMarkdownFile(string filePath)
            => Path.GetExtension(filePath).Equals(".md", StringComparison.OrdinalIgnoreCase);

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

        /// <summary>
        /// Parses a line range string (e.g., "1-10") into start and end line numbers.
        /// </summary>
        /// <param name="lineRange">The line range string to parse.</param>
        /// <param name="startLine">The parsed start line number (1-based).</param>
        /// <param name="endLine">The parsed end line number (1-based, inclusive).</param>
        /// <returns><c>true</c> if parsing was successful; otherwise, <c>false</c>.</returns>
        internal bool TryParseLineRange(string lineRange, out int startLine, out int endLine)
        {
            startLine = 0;
            endLine = 0;

            if (string.IsNullOrWhiteSpace(lineRange))
            {
                return false;
            }

            var parts = lineRange.Split('-');
            if (parts.Length != 2)
            {
                return false;
            }

            if (!int.TryParse(parts[0].Trim(), out startLine) ||
                !int.TryParse(parts[1].Trim(), out endLine))
            {
                return false;
            }

            return startLine > 0 && endLine >= startLine;
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

        /// <summary>
        /// Cleans common translation artifacts where relative paths and links on images are distorted.
        /// </summary>
        private static string CleanTranslationArtifacts(string translated)
            => translated.Replace("! [", "![").Replace("] (", "](").Replace("](.. /", "](../");

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

        private static readonly string[] SupportedExtensions = [".md", ".yml"];

        private string[] FindAllTranslatableFiles(string rootDirectory)
            => SupportedExtensions
                .SelectMany(ext => fileService.GetFiles(rootDirectory, $"*{ext}", SearchOption.AllDirectories))
                .ToArray();

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

        private bool TranslateFile(
            string inputFile,
            string sourceLang,
            string outputFile,
            string targetLang,
            ITranslationMode mode)
        {
            try
            {
                messageHelper.Verbose(mode.FormatStartMessage(inputFile, outputFile, sourceLang, targetLang));

                string content = mode.ReadContent(fileService, inputFile);
                if (string.IsNullOrEmpty(content))
                {
                    string errorMessage = mode.GetNoContentErrorMessage();
                    if (errorMessage != null)
                    {
                        messageHelper.Error(errorMessage);
                    }

                    returnValue = 1;
                    return false;
                }

                string translated = IsMarkdownFile(inputFile)
                    ? TranslateMarkdownContent(content, sourceLang, targetLang)
                    : TranslatePlainTextContent(content, sourceLang, targetLang);

                Console.WriteLine();

                // Save the file
                mode.WriteContent(fileService, outputFile, translated);
                messageHelper.Verbose(mode.FormatCompletionMessage(outputFile));

                return true;
            }
#pragma warning disable CA1031 // Quite a lot of exceptions can happen here
            catch (Exception ex)
#pragma warning restore CA1031 // So catching all of them rather than a long list of individual ones
            {
                messageHelper.Error($"ERROR processing {inputFile}: {ex.Message}");
                returnValue = 1;
                return false;
            }
        }

        private string TranslateMarkdownContent(string content, string sourceLang, string targetLang)
        {
            string translated = TransformMarkdown(content, markdownPipeline, value =>
                ProcessMarkdownSegment(value, sourceLang, targetLang));

            return CleanTranslationArtifacts(translated);
        }

        private string TranslatePlainTextContent(string content, string sourceLang, string targetLang)
            => CleanTranslationArtifacts(ProcessMarkdownSegment(content, sourceLang, targetLang));

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

        private int RunLineRangeTranslation()
        {
            // Validate required options for line range mode
            if (string.IsNullOrEmpty(options.SourceFile))
            {
                messageHelper.Error("ERROR: --sourcefile is required when using --lines option.");
                return 1;
            }

            if (!TryParseLineRange(options.LineRange, out int startLine, out int endLine))
            {
                messageHelper.Error("ERROR: Invalid line range format. Use format like '1-10' or '5-20'.");
                return 1;
            }

            if (!fileService.FileExists(options.SourceFile))
            {
                messageHelper.Error($"ERROR: Source file '{options.SourceFile}' doesn't exist.");
                return 1;
            }

            string rootDirectory = options.DocFolder;
            var allLanguagesDirectories = FindAllRootLanguages(rootDirectory);

            // Find the source language directory by checking which language directory contains the source file
            string normalizedSourceFile = options.SourceFile.Replace('\\', '/');
            string sourceDir = allLanguagesDirectories
                .Select(dir => dir.Replace('\\', '/'))
                .FirstOrDefault(dir => normalizedSourceFile.StartsWith(dir, StringComparison.OrdinalIgnoreCase));

            if (sourceDir == null)
            {
                string detectedDirs = allLanguagesDirectories.Length > 0
                    ? string.Join(", ", allLanguagesDirectories)
                    : "none";
                messageHelper.Error($"ERROR: Source file '{options.SourceFile}' is not within any language directory. Detected language directories: {detectedDirs}");
                return 1;
            }

            string sourceLang = GetLanguageCodeFromPath(sourceDir);
            var lineRangeMode = new LineRangeTranslationMode(startLine, endLine);
            int numberOfFiles = 0;

            foreach (var langDir in allLanguagesDirectories)
            {
                string normalizedLangDir = langDir.Replace('\\', '/');
                string targetLang = GetLanguageCodeFromPath(langDir);

                // Skip the source language directory
                if (normalizedLangDir.Equals(sourceDir, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                // Determine target file path by replacing only the language directory prefix
                string targetFile;
                if (normalizedSourceFile.StartsWith(sourceDir, StringComparison.OrdinalIgnoreCase))
                {
                    targetFile = normalizedLangDir + normalizedSourceFile.Substring(sourceDir.Length);
                }
                else
                {
                    // Fallback: if the source file path does not start with the expected sourceDir,
                    // leave it unchanged rather than performing a global replacement.
                    targetFile = normalizedSourceFile;
                }

                if (!fileService.FileExists(targetFile))
                {
                    if (options.CheckOnly)
                    {
                        messageHelper.Error($"ERROR: Target file '{targetFile}' doesn't exist.");
                        numberOfFiles++;
                        returnValue = 1;
                    }
                    else
                    {
                        messageHelper.Warning($"WARNING: Target file '{targetFile}' doesn't exist. Skipping.");
                    }

                    continue;
                }

                if (options.CheckOnly)
                {
                    // Show what would be translated
                    messageHelper.Verbose($"Would translate lines {startLine}-{endLine} from '{options.SourceFile}' to '{targetFile}' [{sourceLang} to {targetLang}]");
                    string[] sourceLines = fileService.ReadLines(options.SourceFile, startLine, endLine);
                    for (int i = 0; i < sourceLines.Length; i++)
                    {
                        messageHelper.Verbose($"  Line {startLine + i}: {sourceLines[i]}");
                    }

                    numberOfFiles++;
                }
                else if (TranslateFile(options.SourceFile, sourceLang, targetFile, targetLang, lineRangeMode))
                {
                    numberOfFiles++;
                }
            }

            PrintLineRangeCompletionMessage(numberOfFiles);
            return returnValue;
        }

        private void PrintLineRangeCompletionMessage(int numberOfFiles)
        {
            string finalOutput = "Process finished.";
            if (options.CheckOnly && returnValue != 0)
            {
                finalOutput += $" {numberOfFiles} target file(s) missing. Please check the previous lines.";
            }
            else if (options.CheckOnly && numberOfFiles > 0)
            {
                finalOutput += $" {numberOfFiles} file(s) would be updated with translated line range.";
            }
            else if (numberOfFiles > 0)
            {
                finalOutput += $" {numberOfFiles} file(s) updated with translated line range.";
            }
            else
            {
                finalOutput += " No files were updated.";
            }

            Console.WriteLine(finalOutput);
        }
    }
}
