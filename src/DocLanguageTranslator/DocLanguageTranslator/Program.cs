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
            var docFolderOption = new Option<string>(
                aliases: ["--docfolder", "-d"],
                description: "Folder containing the documents.")
            {
                IsRequired = true,
            };

            var verboseOption = new Option<bool>(
                aliases: ["--verbose", "-v"],
                description: "Show verbose messages.",
                getDefaultValue: () => false);

            var keyOption = new Option<string>(
                aliases: ["--key", "-k"],
                description: "The translator Azure Cognitive Services key.");

            var locationOption = new Option<string>(
                aliases: ["--location", "-l"],
                description: "The translator Azure Cognitive Services location.",
                getDefaultValue: () => "westeurope");

            var sourceLanguageOption = new Option<string>(
                aliases: ["--source", "-s"],
                description: "The source language of files to use for missing translations.");

            var checkOnlyOption = new Option<bool>(
                aliases: ["--check", "-c"],
                description: "Check missing files in structure only.",
                getDefaultValue: () => false);

            var sourceFileOption = new Option<string>(
                aliases: ["--sourcefile", "-f"],
                description: "The source file path for line range translation.");

            var lineRangeOption = new Option<string>(
                aliases: ["--lines", "-r"],
                description: "The range of lines to translate (e.g., '1-10' or '5-20'). Requires --sourcefile.");

            // Add options to root command
            rootCommand.AddOption(docFolderOption);
            rootCommand.AddOption(verboseOption);
            rootCommand.AddOption(keyOption);
            rootCommand.AddOption(locationOption);
            rootCommand.AddOption(sourceLanguageOption);
            rootCommand.AddOption(checkOnlyOption);
            rootCommand.AddOption(sourceFileOption);
            rootCommand.AddOption(lineRangeOption);

            // Set command handler
            rootCommand.SetHandler(context =>
            {
                CommandlineOptions options = new CommandlineOptions
                {
                    DocFolder = context.ParseResult.GetValueForOption(docFolderOption),
                    Verbose = context.ParseResult.GetValueForOption(verboseOption),
                    Key = context.ParseResult.GetValueForOption(keyOption),
                    Location = context.ParseResult.GetValueForOption(locationOption),
                    SourceLanguage = context.ParseResult.GetValueForOption(sourceLanguageOption),
                    CheckOnly = context.ParseResult.GetValueForOption(checkOnlyOption),
                    SourceFile = context.ParseResult.GetValueForOption(sourceFileOption),
                    LineRange = context.ParseResult.GetValueForOption(lineRangeOption),
                };

                context.ExitCode = RunLogic(options);
            });

            // Parse and execute
            int returnValue = await rootCommand.InvokeAsync(args);

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
