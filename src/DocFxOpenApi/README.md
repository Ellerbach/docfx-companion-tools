# 🔄 DocFxOpenApi

Make modern OpenAPI specifications consumable by DocFX.

[![NuGet](https://img.shields.io/nuget/v/DocFxOpenApi)](https://www.nuget.org/packages/DocFxOpenApi)
[![NuGet downloads](https://img.shields.io/nuget/dt/DocFxOpenApi)](https://www.nuget.org/packages/DocFxOpenApi)

DocFxOpenApi converts [OpenAPI](https://www.openapis.org/) v2 or v3 JSON/YAML into the OpenAPI v2 JSON format expected by DocFX. Convert one specification or process a complete folder tree as part of an automated documentation build.

## Support the project

If DocFxOpenApi improves your documentation workflow, you can [sponsor ongoing development and maintenance](https://github.com/sponsors/ellerbach).

## Highlights

- Read OpenAPI v2 or v3 specifications in JSON or YAML format.
- Convert a single file or every specification beneath a folder.
- Generate missing operation IDs required by DocFX.
- Write converted specifications to the source tree or a separate output folder.

## Install

DocFxOpenApi is built for .NET 10 and expects the .NET 10 runtime to be installed.

```shell
dotnet tool install --global DocFxOpenApi
```

> [!TIP]
> If .NET 10 is not installed but a newer major runtime is available, add `--roll-forward Major` before the tool arguments (for example, `DocFxOpenApi --roll-forward Major --help`) or set the `DOTNET_ROLL_FORWARD` environment variable to `Major`.

## Usage

```text
DocFxOpenApi -s <specs folder> [-o <output folder>] [-v] [-g]
  -s, --specsource      Required. Folder or file containing the OpenAPI specification.
  -o, --outputfolder	Folder to write the resulting specifications in.
  -v, --verbose         Show verbose messages.
  -g, --genOpId         Generate missing OperationId fields, required by DocFx.
  --help                Display this help screen.
  --version             Display version information.
```

When a folder is provided to the `specsource` parameter, the tool converts all `*.json`, `*.yaml`, `*.yml` files in the folder and its subfolders. When a file is provided, the tool converts only that file.
It supports JSON or YAML-format, OpenAPI v2 or v3 (including 3.0.1) format files.

If the `-o or --outputfolder` is not provided, the output folder is set to the input specs folder.


If normal return code of the tool is 0, but on error it returns 1.

## Warnings, errors and verbose

If the tool encounters situations that might need some action, a warning is written to the output. The table of contents is still created.

If the tool encounters an error, an error message is written to the output. The table of contents will not be created. The tool will return error code 1.

If you want to trace what the tool is doing, use the `-v or verbose` flag to output all details of processing the files and folders and creating the table of contents.

## Limitations and workarounds

- DocFX only supports generating documentation [from OpenAPI v2 JSON files](https://dotnet.github.io/docfx/tutorial/intro_rest_api_documentation.html) as of May 2021. Therefore the utility converts input files into that format.
- DocFX [does not include type definitions](https://github.com/dotnet/docfx/issues/2072) as of May 2021.
- The OpenAPI v2 format does not allow providing multiple examples for result payloads. OpenAPI v3 allows providing either a single example or a collection of examples. If a collection of examples is provided, the utility uses the first example as an example in the output file.
