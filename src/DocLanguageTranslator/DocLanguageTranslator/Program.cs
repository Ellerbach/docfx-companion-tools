// Licensed to DocFX Companion Tools and contributors under one or more agreements.
// DocFX Companion Tools and contributors licenses this file to you under the MIT license.
using System.CommandLine;
using DocFXLanguageGenerator.Domain;
using DocFXLanguageGenerator.Helpers;
using DocLanguageTranslator.FileService;
using DocLanguageTranslator.TranslationService;

namespace DocFXLanguageGenerator
{
    /// <summary>
    /// The core program.
    /// </summary>
    internal class Program
    {
        private static async Task<int> Main(string[] args)
        {
            // Create root command with description
            var rootCommand = new RootCommand("Generates localized versions of DocFX documentation");

            // Define options
            var docFolderOption = new Option<string>("--docfolder", "-d")
            {
                Description = "Folder containing the documents.",
                Required = true,
            };

            var verboseOption = new Option<bool>("--verbose", "-v")
            {
                Description = "Show verbose messages.",
                Required = false,
                DefaultValueFactory = _ => false,
            };

            var keyOption = new Option<string>("--key", "-k")
            {
                Description = "The translator Azure Cognitive Services key.",
                Required = false,
            };

            var locationOption = new Option<string>("--location", "-l")
            {
                Description = "The translator Azure Cognitive Services location.",
                Required = false,
                DefaultValueFactory = _ => "westeurope",
            };

            var sourceLanguageOption = new Option<string>("--source", "-s")
            {
                Description = "The source language of files to use for missing translations.",
                Required = false,
            };

            var checkOnlyOption = new Option<bool>("--check", "-c")
            {
                Description = "Check missing files in structure only.",
                Required = false,
                DefaultValueFactory = _ => false,
            };

            var sourceFileOption = new Option<string>("--sourcefile", "-f")
            {
                Description = "The source file path for line range translation.",
                Required = false,
            };

            var languagesOption = new Option<string[]>("--languages", "-t")
            {
                Description = "One or more target language codes to translate to (e.g., 'de' 'fr' 'zh-Hans'). If not specified, languages are auto-discovered from folder names.",
                AllowMultipleArgumentsPerToken = true,
            };

            var lineRangeOption = new Option<string>("--lines", "-r")
            {
                Description = "The range of lines to translate (e.g., '1-10' or '5-20'). Requires --sourcefile.",
                Required = false,
            };

            var insertLinesOption = new Option<bool>("--insert", "-i")
            {
                Description = "Insert translated lines at the specified position instead of replacing existing lines. Requires --lines.",
                Required = false,
                DefaultValueFactory = _ => false,
            };

            // Add options to root command
            rootCommand.Options.Add(docFolderOption);
            rootCommand.Options.Add(verboseOption);
            rootCommand.Options.Add(keyOption);
            rootCommand.Options.Add(locationOption);
            rootCommand.Options.Add(sourceLanguageOption);
            rootCommand.Options.Add(checkOnlyOption);
            rootCommand.Options.Add(sourceFileOption);
            rootCommand.Options.Add(languagesOption);
            rootCommand.Options.Add(lineRangeOption);
            rootCommand.Options.Add(insertLinesOption);

            // Set command handler
            rootCommand.SetAction(context =>
            {
                CommandlineOptions options = new CommandlineOptions
                {
                    DocFolder = context.GetValue(docFolderOption),
                    Verbose = context.GetValue(verboseOption),
                    Key = context.GetValue(keyOption),
                    Location = context.GetValue(locationOption),
                    SourceLanguage = context.GetValue(sourceLanguageOption),
                    TargetLanguages = context.GetValue(languagesOption),
                    CheckOnly = context.GetValue(checkOnlyOption),
                    SourceFile = context.GetValue(sourceFileOption),
                    LineRange = context.GetValue(lineRangeOption),
                    InsertLines = context.GetValue(insertLinesOption),
                };

                return RunLogic(options);
            });

            // Parse and execute
            int returnValue = await rootCommand.Parse(args).InvokeAsync();

            Console.WriteLine($"Exit with return code {returnValue}");

            return returnValue;
        }

        private static int RunLogic(CommandlineOptions options)
        {
            var fileService = new FileService();
            var translationServie = new TranslationService(
                options.Key,
                options.Location);
            var messageHelper = new MessageHelper(options);
            var generator = new DocFxLanguageGenerator(
                options,
                fileService,
                translationServie,
                messageHelper);

            return generator.Run();
        }
    }
}
