# OpenAPI specification converter for DocFX

This tool converts existing [OpenAPI](https://www.openapis.org/) specification files into the format compatible with DocFX (OpenAPI v2 JSON files). It allows DocFX to generate HTML pages from the OpenAPI specification. OpenAPI is also known as [Swagger](https://swagger.io/).

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
