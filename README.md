# DocFX Companion Tools

This repository contains a series of tools, templates, tips and tricks to make your [DocFX](https://dotnet.github.io/docfx/) life even better.

## Tools

* [DocFxTocGenerator](./src/DocFxTocGenerator): generate a Table of Contents (TOC) in YAML format for DocFX. It has features like the ability to configure the order of files and the names of documents and folders.
* [DocLinkChecker](./src/DocLinkChecker): validate links in documents and check for orphaned attachments in the `.attachments` folder. The tool indicates whether there are errors or warnings, so it can be used in a CI pipeline. It can also clean up orphaned attachments automatically. And it can validate table syntax.
* [DocLanguageTranslator](./src/DocLanguageTranslator): allows to generate and translate automatically missing files or identify missing files in multi language pattern directories.

## Install

The tools can be installed by dowloading the zip-file of a [release](https://github.com/Ellerbach/docfx-companion-tools/releases) or use [Chocolatey](https://chocolatey.org/install) like this:

```shell
choco install docfx-companion-tools
```

## CI Pipeline samples

* [Documentation validation pipeline](./PipelineExamples/documentation-validation.yml): a sample pipeline to use the [DocFxTocGenerator](./src/DocFxTocGenerator) for generating the table of contents and DocFx to generate a website. This sample will also publish to an Azure App Service.
* [Documentation build pipeline](./PipelineExamples/documentation-build.yml): a sample pipeline to use [markdownlint](https://github.com/markdownlint/markdownlint) to validate markdown style and the [DocLinkChecker](./src/DocLinkChecker) to validate the links and attachments.

## Documentation

* [Guidelines on how to use Markdownlint](./DocExamples/docs/markdownlint.md) for your developers.
* [Guidelines for creating Markdown docs](./DocExamples/docs/markdown-creation.md) for your developers. This contains patterns as well as tips and tricks.
* [Guidelines for end user documentation](./DocExamples/docs/enduser-documentation.md) for your developers.
* Specific elements to add and consider for [proper usage and support for Mermaid](./DocExamples/docs/ui-specific-elements.md).

## License

Please read the main [license file](LICENSE) and the sub folder license files and [3rd party notice](THIRD-PARTY-NOTICES.TXT). Most of those tools are coming from a work done with [ZF](https://www.zf.com/).
