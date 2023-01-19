// Licensed to DocFX Companion Tools and contributors under one or more agreements.
// DocFX Companion Tools and contributors licenses this file to you under the MIT license.

namespace DocFxOpenApi
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using CommandLine;
    using global::DocFxOpenApi.Domain;
    using global::DocFxOpenApi.Helpers;
    using Microsoft.OpenApi;
    using Microsoft.OpenApi.Extensions;
    using Microsoft.OpenApi.Models;
    using Microsoft.OpenApi.Readers;

    /// <summary>
    /// Open API file converter to V2 JSON files.
    /// </summary>
    internal class DocFxOpenApi
    {
        private const OpenApiSpecVersion OutputVersion = OpenApiSpecVersion.OpenApi2_0;
        private static readonly string[] _openApiFileExtensions = { "json", "yaml", "yml" };

        private static int _returnvalue;
        private CommandlineOptions _options;
        private MessageHelper _message;

        private DocFxOpenApi(CommandlineOptions thisOptions)
        {
            _options = thisOptions;
            _message = new MessageHelper(thisOptions);
        }

        /// <summary>
        /// Main entry point.
        /// </summary>
        /// <param name="args">Commandline options described in <see cref="CommandlineOptions"/> class.</param>
        /// <returns>0 if successful, 1 on error.</returns>
        private static int Main(string[] args)
        {
            Parser.Default.ParseArguments<CommandlineOptions>(args)
                .WithParsed(RunLogic)
                .WithNotParsed(HandleErrors);

            Console.WriteLine($"Exit with return code {_returnvalue}");

            return _returnvalue;
        }

        /// <summary>
        /// Run the logic of the app with the given parameters.
        /// Given folders are checked if they exist.
        /// </summary>
        /// <param name="o">Parsed commandline options.</param>
        private static void RunLogic(CommandlineOptions o)
        {
            new DocFxOpenApi(o).RunLogic();
        }

        /// <summary>
        /// On parameter errors, we set the returnvalue to 1 to indicated an error.
        /// </summary>
        /// <param name="errors">List or errors (ignored).</param>
        private static void HandleErrors(IEnumerable<Error> errors)
        {
            _returnvalue = 1;
        }

        private void RunLogic()
        {
            if (string.IsNullOrEmpty(_options.OutputFolder))
            {
                _options.OutputFolder = _options.SpecFolder;
            }

            _message.Verbose($"Specification folder: {_options.SpecFolder}");
            _message.Verbose($"Output folder       : {_options.OutputFolder}");
            _message.Verbose($"Verbose             : {_options.Verbose}");

            if (!Directory.Exists(_options.SpecFolder))
            {
                _message.Error($"ERROR: Specification folder '{_options.SpecFolder}' doesn't exist.");
                _returnvalue = 1;
                return;
            }

            Directory.CreateDirectory(_options.OutputFolder!);

            this.ConvertOpenApiFiles();
        }

        private void ConvertOpenApiFiles()
        {
            foreach (var extension in _openApiFileExtensions)
            {
                this.ConvertOpenApiFiles(extension);
            }
        }

        private void ConvertOpenApiFiles(string extension)
        {
            foreach (var file in Directory.GetFiles(
                _options.SpecFolder!,
                $"*.{extension}",
                new EnumerationOptions
                {
                    MatchCasing = MatchCasing.CaseInsensitive,
                    RecurseSubdirectories = true,
                }))
            {
                this.ConvertOpenApiFile(file);
            }
        }

        private void ConvertOpenApiFile(string inputSpecFile)
        {
            _message.Verbose($"Reading OpenAPI file '{inputSpecFile}'");
            using var stream = File.OpenRead(inputSpecFile);
            var document = new OpenApiStreamReader().Read(stream, out var diagnostic);

            if (diagnostic.Errors.Any())
            {
                _message.Error($"ERROR: Not a valid OpenAPI v2 or v3 specification");
                foreach (var error in diagnostic.Errors)
                {
                    _message.Error(error.ToString());
                }

                _returnvalue = 1;
                return;
            }

            _message.Verbose($"Input OpenAPI version '{diagnostic.SpecificationVersion}'");

            foreach (var (pathName, path) in document.Paths)
            {
                foreach (var (operationType, operation) in path.Operations)
                {
                    var description = $"{pathName} {operationType}";

                    foreach (var (responseType, response) in operation.Responses)
                    {
                        foreach (var (mediaType, content) in response.Content)
                        {
                            this.CreateSingleExampleFromMultipleExamples(content, $"{description} response {responseType} {mediaType}");
                        }
                    }

                    foreach (var parameter in operation.Parameters)
                    {
                        foreach (var (mediaType, content) in parameter.Content)
                        {
                            this.CreateSingleExampleFromMultipleExamples(content, $"{description} parameter {parameter.Name} {mediaType}");
                        }
                    }

                    if (operation.RequestBody is not null)
                    {
                        foreach (var (mediaType, content) in operation.RequestBody.Content)
                        {
                            this.CreateSingleExampleFromMultipleExamples(content, $"{description} requestBody {mediaType}");

                            if (content.Example is not null && content.Schema is not null && content.Schema.Example is null)
                            {
                                _message.Verbose($"[OpenAPIv2 compatibility] Setting type example from sample requestBody example for {content.Schema.Reference?.ReferenceV2 ?? "item"} from {operation.OperationId}");
                                content.Schema.Example = content.Example;
                            }
                        }
                    }
                }
            }

            var outputFileName = Path.ChangeExtension(Path.GetFileName(inputSpecFile), ".swagger.json");
            var outputFile = Path.Combine(_options.OutputFolder!, outputFileName);
            _message.Verbose($"Writing output file '{outputFile}' as version '{OutputVersion}'");
            using FileStream fs = File.Create(outputFile);
            document.Serialize(fs, OutputVersion, OpenApiFormat.Json);
        }

        private void CreateSingleExampleFromMultipleExamples(OpenApiMediaType content, string description)
        {
            if (content.Example is null && content.Examples.Any())
            {
                _message.Verbose($"[OpenAPIv2 compatibility] Setting example from first of multiple OpenAPIv3 examples for {description}");
                content.Example = content.Examples.Values.First().Value;
            }
        }
    }
}
