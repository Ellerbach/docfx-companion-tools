# Documentation Assembler Tool

This tool can be used to assemble documentation from various locations on disk and make sure all links still work.

## Usage

```text
DocAssembler [command] [options]

Options:
  --workingfolder <workingfolder>  The working folder. Default is the current folder.
  --config <config> (REQUIRED)     The configuration file for the assembled documentation.
  --outfolder <outfolder>          Override the output folder for the assembled documentation in the config file.
  --cleanup-output                 Cleanup the output folder before generating. NOTE: This will delete all folders and files!
  -v, --verbose                    Show verbose messages of the process.
  --version                        Show version information
  -?, -h, --help                   Show help and usage information

Commands:
  init  Intialize a configuration file in the current directory if it doesn't exist yet.
```

If normal return code of the tool is 0, but on error it returns 1.

Return values:
  0 - successful.
  1 - some warnings, but process could be completed.
  2 - a fatal error occurred.

## Warnings, errors and verbose

If the tool encounters situations that might need some action, a warning is written to the output. Documentation is still assembled. If the tool encounters an error, an error message is written to the output. Documentation might not be assembled or complete.

If you want to trace what the tool is doing, use the `-v or --verbose` flag to output all details of processing the files and folders and assembling content.

## Overall process

The overall process of this tool is:

1. Content inventory - retrieve all folders and files that can be found with the configured content sets. In this stage we already calculate the new path in the configured output folder. Url replacements when configured are executed here (see [`Replacement`](#replacement) for more details).
2. If configured, delete the existing output folder.
3. Copy over all found files to the newly calculated location. Content replacements when configured are executed here. We also change links in markdown files to the new location of the referenced files, unless it's a 'raw copy'. Referenced files that are not found in the content sets are prefixed with the configured prefix.

The basic idea is to define a content set that will be copied to the destination folder. The reason to do this, is because we now have the possibility to completely restructure the documentation, but also apply changes in the content. In a CI/CD process this can be used to assemble all documentation to prepare it for the use of the [DocFxTocGenerator](https://github.com/Ellerbach/docfx-companion-tools/blob/main/src/DocFxTocGenerator) to generate the table of content and then use tools as [DocFx](https://dotnet.github.io/docfx/) to generate a documentation website. The tool expects the content set to be validated for valid links. This can be done using the [DocLinkChecker](https://github.com/Ellerbach/docfx-companion-tools/blob/main/src/DocLinkChecker).

## Configuration file

A configuration file is used for settings. Command line parameters will overwrite these settings if provided.

An initialized configuration file called `.docassembler.json` can be generated in the working directory by using the command:

```shell
DocAssembler init
```

If a `.docassembler.json` file already exists in the working directory, an error is given that it will not be overwritten. The generated structure will look like this:

```json
{
  "dest": "out",
  "externalFilePrefix": "https://github.com/example/blob/main/",
  "content": [
    {
      "src": ".docfx",
      "files": [
        "**"
      ],
      "rawCopy": true
    },
    {
      "src": "docs",
      "files": [
        "**"
      ]
    },
    {
      "src": "backend",
      "dest": "services",
      "files": [
        "**/docs/**"
      ],
      "urlReplacements": [
        {
          "expression": "/[Dd]ocs/",
          "value": "/"
        }
      ]
    }
  ]
}
```

### General settings

In the general settings these properties can be set:

| Property              | Description                                                  |
| --------------------- | ------------------------------------------------------------ |
| `dest` (Required)     | Destination sub-folder in the working folder to copy the assembled documentation to. This value can be overruled with the `--outfolder` command line argument. |
| `urlReplacements`     | A global collection of [`Replacement`](#replacement) objects to use across content sets for URL paths. These replacements are applied to calculated destination paths for files in the content sets. This can be used to modify the path. The generated template removes /docs/ from paths and replaces it by a /. If a content set has `urlReplacements` configured, it overrules these global ones. More information can be found under [`Replacement`](#replacement). |
| `contentReplacements` | A global collection of [`Replacement`](#replacement) objects to use across content sets for content of files. These replacements are applied to all content of markdown files in the content sets. This can be used to modify for instance URLs or other content items. If a content set has `contentReplacements` configured, it overrules these global ones. More information can be found under [`Replacement`](#replacement). |
| `externalFilePrefix`  | The global prefix to use for all referenced files in all content sets that are not part of the documentation, like source files. This prefix is used in combination with the sub-path from the working folder. If a content set has `externalFilePrefix` configured, it overrules this global one. |
| `content` (Required)  | A collection of [`Content`](#content) objects to define all content sets to assemble. |

### `Replacement`

A replacement definition has these properties:

| Property     | Description                                                  |
| ------------ | ------------------------------------------------------------ |
| `expression` | A regular expression to find specific text. |
| `value` | The value that replaces the found text. Named matched subexpressions can be used here as well as explained below. |

This type is used in collections for URL replacements or content replacements. They are applied one after another, starting with the first entry. The regular expression is used to find text that will be replaced by the value. The expressions are regular expression as described in [.NET Regular Expressions - .NET  Microsoft Learn](https://learn.microsoft.com/en-us/dotnet/standard/base-types/regular-expressions). Examples can be found there as well. There are websites like [regex101: build, test, and debug regex](https://regex101.com/) to build, debug and validate the expression you need.

#### Using named matched subexpressions

Sometimes you want to find specific content, but also reuse parts of it in the value replacement. An example would be to find all `AB#1234` notations and replace it by a URL to the referenced Azure Boards work-item or GitHub item. But in this case we want to use the ID (1234) in the value. To do that, you can use [Named matched subexpressions](https://learn.microsoft.com/en-us/dotnet/standard/base-types/grouping-constructs-in-regular-expressions#named-matched-subexpressions).

This expression could be used to find all those references:

```regex
(?<pre>[$\\s])AB#(?<id>[0-9]{3,6})
```

As we don't want to find a link like `[AB#1234](https://...)`, we look for all AB# references that are at the start of a line (using the `$` tag) or are prefixed by a whitespace (using the `\s` tag). As we need to keep that prefix in place, we capture it as a named subexpression called `pre`.

> [!NOTE]
>
> As the expression is configured in a string in a JSON file, special characters like back-slashes need to be escaped by an (extra) back-slash.

The second part is to get the numbers after the AB# text. This is configured here to be between 3 and 6 characters. We also want to reuse this ID in the value, so we capture it as a named subexpression called `id`.

In the value we can reuse these named subexpression like this:

```text
${pre}[AB#${id}](https://dev.azure.com/[your organization]/_workitems/edit/${id})
```

We start with the `pre` value, after which we build a markdown link with AB# combined with the `id` as the text and the `id` as parameter for the URL. We reference an Azure Board work item here. Of course you need to replace the `[your organization]` with the proper value for your ADO environment here.

### `Content`

The content is defined with these properties:

| Property         | Description                                                  |
| ---------------- | ------------------------------------------------------------ |
| `src` (Required) | The source sub-folder relative to the working folder.        |
| `dest`           | An optional destination sub-folder path in the output folder. If this is not given, the relative path to the source folder is used. |
| `files`          | This is a  [File Globbing in .NET](https://learn.microsoft.com/en-us/dotnet/core/extensions/file-globbing) pattern. Make sure to also include all needed files for documentation like images and assets. |
| `exclude`        | This is a  [File Globbing in .NET](https://learn.microsoft.com/en-us/dotnet/core/extensions/file-globbing) pattern. This can be used to exclude specific folders or files from the content set. |
| `rawCopy` | If this value is `true` then we don't look for any links in markdown files and therefor also don't fix them. This can be used for raw content you want to include in the documentation set like `.docfx.json`, templates and such. |
| `urlReplacements`     | A collection of [`Replacement`](#replacement) objects to use for URL paths in this content set, overruling any global setting. These replacements are applied to calculated destination paths for files in the content sets. This can be used to modify the path. The generated template removes /docs/ from paths and replaces it by a /. More information can be found under [`Replacement`](#replacement). |
| `contentReplacements` | A collection of [`Replacement`](#replacement) objects to use for content of files in this content set, overruling any global setting. These replacements are applied to all content of markdown files in the content sets. This can be used to modify for instance URLs or other content items. More information can be found under [`Replacement`](#replacement). |
| `externalFilePrefix`  | The prefix to use for all referenced files in this content sets that are not part of the complete documentation set, like source files. It overrides any global prefix. This prefix is used in combination with the sub-path from the working folder. |


