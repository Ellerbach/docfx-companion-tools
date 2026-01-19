// Licensed to DocFX Companion Tools and contributors under one or more agreements.
// DocFX Companion Tools and contributors licenses this file to you under the MIT license.
using CommandLine;
using global::DocFxOpenApi.Domain;
using global::DocFxOpenApi.Helpers;
using Microsoft.OpenApi;
using Microsoft.OpenApi.Reader;

namespace DocFxOpenApi
{
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
        private static async Task<int> Main(string[] args)
        {
            var parsedArguments = Parser.Default.ParseArguments<CommandlineOptions>(args);

            var intermediateResult = await parsedArguments.WithParsedAsync(RunLogicAsync).ConfigureAwait(false);
            var result = await intermediateResult.WithNotParsedAsync(HandleErrorsAsync).ConfigureAwait(false);

            Console.WriteLine($"Exit with return code {_returnvalue}");

            return _returnvalue;
        }

        /// <summary>
        /// Run the logic of the app with the given parameters.
        /// Given folders are checked if they exist.
        /// </summary>
        /// <param name="o">Parsed commandline options.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        private static async Task RunLogicAsync(CommandlineOptions o)
        {
            await new DocFxOpenApi(o).RunLogic(CancellationToken.None).ConfigureAwait(false);
        }

        /// <summary>
        /// On parameter errors, we set the returnvalue to 1 to indicate an error.
        /// </summary>
        /// <param name="errors">List or errors (ignored).</param>
        private static Task HandleErrorsAsync(IEnumerable<Error> errors)
        {
            _returnvalue = 1;
            return Task.CompletedTask;
        }

        private async Task RunLogic(CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(_options.OutputFolder))
            {
                _options.OutputFolder = _options.SpecFolder ?? Path.GetDirectoryName(_options.SpecFile);
            }

            _message.Verbose($"Specification file/folder: {_options.SpecFolder ?? _options.SpecFile}");
            _message.Verbose($"Output folder       : {_options.OutputFolder}");
            _message.Verbose($"Verbose             : {_options.Verbose}");
            _message.Verbose($"Generate OperationId Members: {_options.GenerateOperationId}");

            if ((_options.SpecFolder ?? _options.SpecFile) == null)
            {
                _message.Error($"ERROR: Specification folder/file '{_options.SpecSource}' doesn't exist.");
                _returnvalue = 1;
                return;
            }

            Directory.CreateDirectory(_options.OutputFolder!);

            await this.ConvertOpenApiFilesAsync(cancellationToken).ConfigureAwait(false);
        }

        private async Task ConvertOpenApiFilesAsync(CancellationToken cancellationToken)
        {
            if (_options.SpecFolder != null)
            {
                foreach (var extension in _openApiFileExtensions)
                {
                    await this.ConvertOpenApiFilesAsync(extension, cancellationToken).ConfigureAwait(false);
                }
            }
            else
            {
                await this.ConvertOpenApiFileAsync(_options.SpecFile!, cancellationToken).ConfigureAwait(false);
            }
        }

        private async Task ConvertOpenApiFilesAsync(string extension, CancellationToken cancellationToken)
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
                await this.ConvertOpenApiFileAsync(file, cancellationToken).ConfigureAwait(false);
            }
        }

        private async Task ConvertOpenApiFileAsync(string inputSpecFile, CancellationToken cancellationToken)
        {
            _message.Verbose($"Reading OpenAPI file '{inputSpecFile}'");
            using var stream = File.OpenRead(inputSpecFile);
            var settings = new OpenApiReaderSettings();
            settings.AddYamlReader();
            var (document, diagnostic) = await OpenApiDocument.LoadAsync(inputSpecFile, settings, cancellationToken).ConfigureAwait(false);

            if (document is null)
            {
                _message.Error($"ERROR: Unable to read OpenAPI specification from file '{inputSpecFile}'");
                _returnvalue = 1;
                return;
            }

            if (diagnostic is null)
            {
                _message.Error($"ERROR: Unable to read diagnostics from OpenAPI specification file '{inputSpecFile}'");
                _returnvalue = 1;
                return;
            }

            if (diagnostic.Errors.Any())
            {
                _message.Error($"ERROR: '{inputSpecFile}' is not a valid OpenAPI v2 or v3+ specification");
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
                if (path.Operations is null)
                {
                    continue;
                }

                foreach (var (operationType, operation) in path.Operations)
                {
                    if (_options.GenerateOperationId && string.IsNullOrWhiteSpace(operation.OperationId))
                    {
                        operation.OperationId = GenerateOperationId(operationType, pathName, operation.Parameters);
                    }

                    var description = $"{pathName} {operationType}";

                    if (operation.Responses is not null)
                    {
                        foreach (var (responseType, response) in operation.Responses)
                        {
                            if (response.Content is null)
                            {
                                continue;
                            }

                            foreach (var (mediaType, content) in response.Content)
                            {
                                this.CreateSingleExampleFromMultipleExamples(content, $"{description} response {responseType} {mediaType}");
                            }
                        }
                    }

                    if (operation.Parameters is not null)
                    {
                        foreach (var parameter in operation.Parameters)
                        {
                            if (parameter.Content is null)
                            {
                                continue;
                            }

                            foreach (var (mediaType, content) in parameter.Content)
                            {
                                this.CreateSingleExampleFromMultipleExamples(content, $"{description} parameter {parameter.Name} {mediaType}");
                            }
                        }
                    }

                    if (operation.RequestBody is { Content.Count: > 0 })
                    {
                        foreach (var (mediaType, content) in operation.RequestBody.Content)
                        {
                            this.CreateSingleExampleFromMultipleExamples(content, $"{description} requestBody {mediaType}");

                            if (content is { Example: not null, Schema: { Example: null } schema })
                            {
                                var (effectiveSchema, id) = schema switch
                                {
                                    OpenApiSchema s => (s, "item"),
                                    OpenApiSchemaReference rs => (rs.RecursiveTarget, rs.Reference.Id),
                                    _ => (null, null),
                                };

                                if (effectiveSchema == null)
                                {
                                    _message.Verbose($"[OpenAPIv2 compatibility] Unable to set type example from sample requestBody example for {id} from {operation.OperationId}");
                                    continue;
                                }

                                _message.Verbose($"[OpenAPIv2 compatibility] Setting type example from sample requestBody example for {id} from {operation.OperationId}");
                                effectiveSchema.Example = content.Example;
                            }
                        }
                    }
                }
            }

            var outputFileName = Path.ChangeExtension(Path.GetFileName(inputSpecFile), ".swagger.json");
            var outputFile = Path.Combine(_options.OutputFolder!, outputFileName);
            _message.Verbose($"Writing output file '{outputFile}' as version '{OutputVersion}'");
            using var fs = File.Create(outputFile);
            await document.SerializeAsJsonAsync(fs, OutputVersion, cancellationToken).ConfigureAwait(false);
        }

        private void CreateSingleExampleFromMultipleExamples(IOpenApiMediaType content, string description)
        {
            if (content is OpenApiMediaType { Example: null } effectiveMediaType && content.Examples is { Count: > 0 })
            {
                _message.Verbose($"[OpenAPIv2 compatibility] Setting example from first of multiple OpenAPIv3 examples for {description}");
                effectiveMediaType.Example = content.Examples.Values.First().Value;
            }
        }

        private string GenerateOperationId(HttpMethod operationType, string pathName, IList<IOpenApiParameter>? parameters)
        {
            return string.Join(string.Empty, SplitPathString(operationType, pathName, parameters));

            static string ToPascalCase(string value)
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    return value;
                }

                return string.Concat(value[0].ToString().ToUpperInvariant(), value.AsSpan(1));
            }

            static IEnumerable<string> SplitPathString(HttpMethod operationType, string path, IList<IOpenApiParameter>? parameters)
            {
                yield return operationType.ToString().ToLowerInvariant();

                string start = path.StartsWith("/api/", StringComparison.InvariantCultureIgnoreCase)
                    ? path[5..]
                    : path;

                foreach (var split in start.Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                {
                    if (split.StartsWith('{'))
                    {
                        break;
                    }

                    yield return ToPascalCase(split);
                }

                if (parameters is not { Count: > 0 })
                {
                    yield break;
                }

                yield return "By";

                foreach (var parameter in parameters.Where(static it => it.In == ParameterLocation.Path && !string.IsNullOrWhiteSpace(it.Name)))
                {
                    yield return ToPascalCase(parameter.Name!);
                }
            }
        }
    }
}
