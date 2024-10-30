# Documentation link checker

This tool can be used to check references in markdown files.

## Usage

```text
DocLinkChecker -d <docs folder> [-vac] [-f <config file>]

One of these options is required. The other can be added to overwrite settings.
-d, --docfolder        Folder containing the documents.
-f, --config           Configuration file.

Other options:
-v, --verbose          Show verbose messages.
-a, --attachments      Check the .attachments folder in the root of the docfolder for unreferenced files.
-c, --cleanup          Remove all unreferenced files from the .attachments folder in the root of the docfolder. Must be used in combination with -a flag.
-t, --table            Check that tables are well formed.
--help                 Display this help screen.
--version              Display version information.
```

If normal return code of the tool is 0, but on error it returns 1.

## Warnings, errors and verbose

The tool will first read all markdown files in the configured documentation root and parse them to extract links, headings and tables. Then it will validate tables and links. Once that is done output is written in the console. When there are errors or warnings, they are written to the output sorted by file path, line number and then column number.

The tool always outputs the version of the used tool and a summary of the result, including the exit code.

The exit codes of the tool are defined in the table below. The exit code can be used to determine if other possible actions are taken or not.

| Exit Code | Description |
| :--- | :--- |
| 0 | Execution has finished successfully |
| 1 | Errors in the command line |
| 3 | Errors in the configuration file |
| 1000 (232 on Linux) | Execution has finished with warnings only |
| 1001 (233 on Linux) | Execution has finished with errors |

If you want to trace what the tool is doing in detail, use the `-v or verbose` flag to output all details of processing the files and folders.

> [!NOTE]
>
> Return codes on Linux are (mostly) truncated as byte, which makes the return codes of 1000 and 1001 be reported as 232 and 233.

## Configuration file

A configuration file can be used for (more) settings used by the tool. Command line parameters will overwrite these settings if provided.

An initialized configuration file called `docfx-companion-tools.json` can be generated in the working directory by using the command:

```shell
DocLinkChecker INIT
```

If a `docfx-companion-tools.json` file already exists in the working directory, an error is given that it will not be overwritten. The main structure looks like this:

```json
{
  "DocumentationFiles": {
    "src": "",
    "Files": [
      "**/*.md"
    ],
    "Exclude": []
  },
  "ResourceFolderNames": [
    ".attachments"
  ],
  "DocLinkChecker": {
    "RelativeLinkStrategy": "All",
    "CheckForOrphanedResources": false,
    "CleanupOrphanedResources": false,
    "ValidatePipeTableFormatting": false,
    "ValidateExternalLinks": false,
    "ConcurrencyLevel": 5,
    "MaxHttpRedirects": 20,
    "ExternalLinkDurationWarning": 3000,
    "WhitelistUrls": [
      "http://localhost"
    ]
  }
}
```

### Files to include or exclude

The configuration file has an entry `DocumentationFiles` that can be partially filled in or can be used in it's full form. The full form looks like this:

```json
  "DocumentationFiles": {
    "src": "",
    "Files": [
      "**/*.md"
    ],
    "Exclude": []
  }
```

These properties are used in combination with the [File Globbing in .NET](https://learn.microsoft.com/en-us/dotnet/core/extensions/file-globbing) pattern.

The `src` property indicates the folder where the documentation hierarchy is located. This value can be overwritten by the `-d <folder>` command line parameter.

The `Files` property is a list of [patterns](https://learn.microsoft.com/en-us/dotnet/core/extensions/file-globbing#pattern-formats) of files to include in the check. If nothing is provided, `**/*.md` is automatically added to include all markdown files.

The `Exclude` property is also a list of [patterns](https://learn.microsoft.com/en-us/dotnet/core/extensions/file-globbing#pattern-formats) of files to exclude in the check.

### Resource folder names

Resources are files that are referenced in markdown files that are not markdown files themselves. Think of images and such.

The `ResourceFolderNames` property is a list of names that can be used as resource folder. When resources are validated, they should be stored in a folder (or subfolder of a folder) with a name defined in this list. By default ".attachments" is defined here, which means that a resource must be something like "..../.attachments/my-image.png" or "..../.attachments/my-topic/my-image.png".

These folders are also checked when orphaned resources are detected or removed. See further down for more details.

### DocLinkChecker settings

Various settings can be provided in the configuration file, specific for the DocLinkChecker. We list them in the table below.

| Setting                        | Purpose                                                      | Default value        |
| ------------------------------ | ------------------------------------------------------------ | -------------------- |
| RelativeLinkStrategy | "All" allows links to all existing files, "SameDocsHierarchyOnly" only allows links to files in the same /docs hierarchy, and "AnyDocsHierarchy" only allows links to files in any /docs hierarchy.  | *All* |
| CheckForOrphanedResources      | If this value is set to **true** the tool checks for files in the folders with a name from the `ResourceFolderNames` list that are not used in any markdown file that was scanned. Those files will be reported with an error. | *false*              |
| CleanupOrphanedResources       | If this value is set to **true** AND if no errors are reported, the tool will delete orphaned resources from all folders with a name from the `ResourceFolderNames` list. | *false*              |
| ValidatePipeTableFormatting    | If this value is set to **true** the tool will validate all pipe table definitions for proper definition. For more information on what's validated, see [Table Validation](#table-validation). | *false*              |
| ValidateExternalLinks          | If this value is set to **true** the tool will validate all web links if they are still valid. For more information on the details, see [Web Links](#web-links). | *false*              |
| ConcurrencyLevel               | This value defines the number of concurrent threads used to validate links, both local and web links. You can use this to improve the performance of the tool, where it will have most impact when web links are validated. | *5*                  |
| ExternalLinkDurationWarning    | When the validation of an external link takes longer than this value, the tool will report a warning. The value is given in milliseconds. This value is only used when external links are validated. | *3000*               |
| WhitelistUrls                  | This is a list of URL's that will be skipped for external link validation. For more details, see [Whitelisting web links](#whitelisting-web-links). | *"http://localhost"* |

## What is validated

The tool uses [markdig](https://github.com/xoofx/markdig) to extract links, tables and headings from markdown files.

### Local references

Links to other markdown files in the same hierarchy are validated to exist. When a file is referenced outside of the documents root an error or warning is reported, depending on the `AllowLinksOutsideDocumentsRoot` setting. When local files are referenced with a full rooted path (like `d:\\git\\project\\docs\\some-file.md`) an error is reported that this is not allowed. Such a reference will be different on other machines.

Links to headings (like `#some-heading` or `./another-file.md#another-heading`) are validated by the tool. If they can't be found, but the file can be found a warning is reported. The link will work to open the document (or page), but it won't locate you somewhere in that document.

### Ignored links

Links like `mailto:` (email address) or `xref:` ([reference link by id](https://dotnet.github.io/docfx/tutorial/links_and_cross_references.html)) are ignored and not validated at all.

### Web Links

Markdown links can be used to reference web links. The tool validates links as web link when the start with:

* *http* or *https*
* *ftp* or *ftps*

When `ValidateExternalLinks` is set to **true** and the link is not excluded by the `WhitelistUrls`, the URL is validated to exist. The tool will report an error for a URL when:

* The URL is not found (404)
* The URL is reported as gone (410)
* The Request URI is too long (414)

Redirects (300-399) are taken as an existing URL. Other status codes (like 401 Unauthorized, 403 Forbidden, 500 Internal Server Error or 503 Service Unavailable and more) are taken as an existing resource, but URLs with those status codes will be reported as a warning.

#### Whitelisting Web Links

It is possible to exclude certain web links from validation. This can be done with the `WhitelistUrls` list. Wildcards (* and ?) can be used to defined a pattern of URLs to skip. 

Examples of valid patterns are:

| Pattern                   | What it means                                                |
| ------------------------- | ------------------------------------------------------------ |
| http://localhost          | All URLs beginning with http://localhost are excluded.       |
| http://localhost:?000/*   | All URLs beginning like http://localhost**:5000**/ are excluded, but an URL beginning with http://localhost**:8080**/ is not. The character-wildcard (?) is used for only 1 character. |
| https://\*.contoso.com/\* | All **secure**  websites (https) of contoso.com are excluded. For instance **https**://documents.contoso.com or **https**://reports.contoso.com. |
| http*://\*.contoso.com/\* | All websites (http or https) of contoso.com are excluded. For instance **http**://demo.contoso.com or **https**://reports.contoso.com. |

If no wildcards are used, the * wildcard is automatically appended to the URL. But if one or more wildcards are used this is not done.

If the link is a web URL, an internal reference (starting with a '#') an e-mail address or a reference to a folder, it's not checked. Other links are checked if they exist in the existing docs hierarchy or on local disc (for code references). Errors are written to the output mentioning the filename, the line number and position in the line. In the check we also decode the references to make sure we properly check HTML encoded strings as well (using %20 for instance).

All references are stored in a table to use in the check of the .attachments folder (with the -a flag). All files in this folder that are not referenced are marked as 'unreferenced'. If the -c flag is provided as well, the files are removed from the .attachments folder.

### Table Validation

[Tables (also known as Pipe Tables)](https://www.markdownguide.org/extended-syntax/#tables) can be used in markdown. Not all renderers act the same with the definitions though. To make sure that all renderers properly render the tables, table validation will check for these things:

1. Do all rows have the same amount of columns? The first row defines the width.
2. Do all rows start and end with a pipe character '|'?
3. Do all columns in the second row (separator line) have at least three dashes per column?

The tool will report each of these errors for tables when `ValidatePipeTableFormatting` is set to **true**.

### Resources

Resources are files that are referenced in markdown files that are not markdown files themselves. Think of images and such. The tool only allows resources to be stored in a folder that is listed in the `ResourceFolderNames` setting. It can also be stored in a sub folder of that folder.

By default ".attachments" is defined in the configuration, which means that a resource must be something like "..../.attachments/my-image.png" or "..../.attachments/my-topic/my-image.png". The `....` indicate that it can be anywhere in the documentation hierarchy.

When checking for orphaned resources, we check resource files in all folders (and it's sub folders) to be referenced by one of the markdown files in the documentation hierarchy. If that's not the case, the tool will report that file as orphaned. This is only done when `CheckForOrphanedResources` is set to **true**. When `CleanupOrphanedResources` is set to **true** as well, the tool will also delete the orphaned resources when no other errors are reported by the scan.
