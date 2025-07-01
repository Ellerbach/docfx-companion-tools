// Licensed to DocFX Companion Tools and contributors under one or more agreements.
// DocFX Companion Tools and contributors licenses this file to you under the MIT license.
using CommandLine;
using DocFXLanguageGenerator.Domain;
using DocLanguageTranslator.FileService;
using DocLanguageTranslator.TranslationService;

namespace DocFXLanguageGenerator
{
    /// <summary>
    /// The core program.
    /// </summary>
    internal class Program
    {
        private static int returnvalue;

        private static int Main(string[] args)
        {
            Parser.Default.ParseArguments<CommandlineOptions>(args)
                   .WithParsed<CommandlineOptions>(RunLogic)
                   .WithNotParsed(HandleErrors);

            Console.WriteLine($"Exit with return code {returnvalue}");

            return returnvalue;
        }

        private static void RunLogic(CommandlineOptions options)
        {
            var fileService = new FileService();
            var translationServie = new TranslationService(
                options.Key,
                options.Location);

            var generator = new DocFxLanguageGenerator(
                options,
                fileService,
                translationServie);

            returnvalue = generator.Run();
        }

        /// <summary>
        /// On parameter errors, we set the returnvalue to 1 to indicated an error.
        /// </summary>
        /// <param name="errors">List or errors (ignored).</param>
        private static void HandleErrors(IEnumerable<CommandLine.Error> errors)
        {
            returnvalue = 1;
        }
    }
}
