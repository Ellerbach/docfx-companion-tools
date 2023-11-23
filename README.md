# DocFX Companion Tools

This repository contains a series of tools, templates, tips and tricks to make your [DocFX](https://dotnet.github.io/docfx/) life even better.

## Tools

* [DocFxTocGenerator](./src/DocFxTocGenerator): generate a Table of Contents (TOC) in YAML format for DocFX. It has features like the ability to configure the order of files and the names of documents and folders.
* [DocLinkChecker](./src/DocLinkChecker): validate links in documents and check for orphaned attachments in the `.attachments` folder. The tool indicates whether there are errors or warnings, so it can be used in a CI pipeline. It can also clean up orphaned attachments automatically. And it can validate table syntax.
* [DocLanguageTranslator](./src/DocLanguageTranslator): allows to generate and translate automatically missing files or identify missing files in multi language pattern directories.
* [DocFxOpenApi](./src/DocFxOpenApi): converts existing [OpenAPI](https://www.openapis.org/) specification files into the format compatible with DocFX (OpenAPI v2 JSON files). It allows DocFX to generate HTML pages from the OpenAPI specification. OpenAPI is also known as [Swagger](https://swagger.io/).

## Creating PR's

The main branch is protected. Features and fixes can be done through PR's only. Make sure you use a proper title for the PR and keep them as small as possible. If you want the PR to pop up in the CHANGELOG, you have to provide one or more labels with the PR. The list of labels that are used:

| Category | Description | Labels |
| --- | --- | --- |
| 🚀 Features | New or modified features | feature, enhancement |
| 🐛 Fixes | All (bug) fixes | fix, bug |
| 📄 Documentation | Documentation additions or changes | documentation |

## Build and Publish

If you have this repo on your local machine, you can run the same scripts for building and packaging as we're using in the workflows. To build the tools use the **build** script. In PowerShell run this command:

```PowerShell
.\build.ps1
```

The result of this script is an output folder containing the executables of all solutions. They are all published as single exe's without the framework. They depend on .NET 5 being installed in the environment. The LICENSE file is copied to the output folder as well. The contents of this folder is then compressed in a zip-file in the root with the name 'tools.zip'.

To package and publish the tools, you must first have run the **build** script. Next you can run the **pack** script we're using from the worklows as well. In PowerShell run this command, where you provide the correct version:

```PowerShell
.\pack.ps1 -publish -version "1.0.0"
```

The script determine the hash of the tools.zip, change the Chocolatey nuspec and install script to contain the hash and the correct versions. Then the Chocolatey package is created. If the **CHOCO_TOKEN** environment variable is set containing the secret to use for Chocolatey publication, the script will also publish the package to Chocolatey. Otherwise a warning is given that the publish step is skipped.

If you omit the -publish parameter, the script will run in develop mode. It will not publish to Chocolatey and it will output the changes of the Chocolatey files for inspection.

> [!NOTE]
> If you run the **pack** script locally, files are changed (*deploy\chocolatey\docfx-companion-tools.nuspec* and *deploy\chocolatey\tools\chocolateyinstall.ps1*). Maybe it's best not to commit that into the repo, although it's not secret information. Next run will overwrite the correct values anyway.

## Version release and publish to Chocolatey

If you have one or more PR's and want to release a new version, just make sure that all PR's are labeled where needed (see above) and merged into main. Run the manual **Release & Publish** workflow manually on the main branch. This will bump the version, create a release and publish a new package to Chocolatey.

## Install

### Chocolatey

The tools can be installed by downloading the zip-file of a [release](https://github.com/Ellerbach/docfx-companion-tools/releases) or use [Chocolatey](https://chocolatey.org/install) like this:

```shell
choco install docfx-companion-tools
```

> [!NOTE]
> The tools expect the .NET Framework 6 to be installed locally. If you need to run them in a framework which is higher,
> add `--roll-forward Major` as a parameter like this:
> `~/.dotnet/tools/DocLinkChecker --roll-forward Major`

### dotnet tool

You can as well install the tools through `dotnet tool`.

```shell
dotnet tool install DocFxTocGenerator -g
dotnet tool install DocLanguageTranslator -g
dotnet tool install DocLinkChecker -g
dotnet tool install DocFxOpenApi -g
```

### usage

Once the tools are installed this way you can use them directly from the command line. For example:

```PowerShell
DocFxTocGenerator -d .\docs -vsi
DocLanguageTranslator -d .\docs\en -k <key> -v
DocLinkChecker -d .\docs -va
```

## CI Pipeline samples

* [Documentation validation pipeline](./PipelineExamples/documentation-validation.yml): a sample pipeline to use the [DocFxTocGenerator](./src/DocFxTocGenerator) for generating the table of contents and DocFx to generate a website. This sample will also publish to an Azure App Service.
* [Documentation build pipeline](./PipelineExamples/documentation-build.yml): a sample pipeline to use [markdownlint](https://github.com/markdownlint/markdownlint) to validate markdown style and the [DocLinkChecker](./src/DocLinkChecker) to validate the links and attachments.

## Docker

Build a Docker image. Below example based on `DocLinkChecker`, adjust `--tag` and `--build-arg` accordantly for the other tools.

```shell
docker build --tag doclinkchecker:latest --build-arg tool=DocLinkChecker -f Dockerfile .
```

Run from `PowerShell`:

```PowerShell
docker run --rm -v ${PWD}:/workspace doclinkchecker:latest -d /workspace
```

Run from Linux/macOS `shell`:

```shell
docker run --rm -v $(pwd):/workspace doclinkchecker:latest -d /workspace
```

## Documentation

* [Guidelines on how to use Markdownlint](./DocExamples/docs/markdownlint.md) for your developers.
* [Guidelines for creating Markdown docs](./DocExamples/docs/markdown-creation.md) for your developers. This contains patterns as well as tips and tricks.
* [Guidelines for end user documentation](./DocExamples/docs/enduser-documentation.md) for your developers.
* Specific elements to add and consider for [proper usage and support for Mermaid](./DocExamples/docs/ui-specific-elements.md).

## License

Please read the main [license file](LICENSE) and the sub folder license files and [3rd party notice](THIRD-PARTY-NOTICES.TXT). Most of those tools are coming from a work done with [ZF](https://www.zf.com/).
