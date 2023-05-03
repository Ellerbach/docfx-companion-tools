# Documentation link checker

This tool can be used to check references in markdown files.

## Usage

```text
DocLinkChecker -d <docs folder> [-vac]

-d, --docfolder       Required. Folder containing the documents.
-v, --verbose         Show verbose messages.
-a, --attachments     Check the .attachments folder in the root of the docfolder for unreferenced files.
-c, --cleanup         Remove all unreferenced files from the .attachments folder in the root of the docfolder. Must be used in combination with -a flag.
-t, --table           Check that tables are well formed.
-s, --skip            RegExp to define which folders or files to skip.
--help                Display this help screen.
--version             Display version information.
```

If normal return code of the tool is 0, but on error it returns 1.

## Warnings, errors and verbose

If the tool encounters situations that might need some action, a warning is written to the output. The table of contents is still created.

If the tool encounters an error, an error message is written to the output. The table of contents will not be created.

| Exit Code | Description                         |
| :-------- | :---------------------------------- |
| 0         | Execution has finished successfully |
| 1         | Arguments Parsing Exception         |
| 2         | Incorrect Table Format              |
| 999       | Unknown Exception                   |

If you want to trace what the tool is doing, use the `-v or verbose` flag to output all details of processing the files and folders and creating the table of contents.

## What it checks for links

The tool will track all use of `[]()`. If the link is a web URL, an internal reference (starting with a '#') an e-mail address or a reference to a folder, it's not checked. Other links are checked if they exist in the existing docs hierarchy or on local disc (for code references). Errors are written to the ouput mentioning the filename, the linenumber and position in the line. In the check we also decode the references to make sure we properly check HTML enccoded strings as well (using %20 for instance).

All references are stored in a table to use in the check of the .attachments folder (with the -a flag). All files in this folder that are not referenced are marked as 'unreferenced'. If the -c flag is provided as well, the files are removed from the .attachments folder.

## What it checks for tables

The tool will check that tables are well formed. Here are the rules to respect:

- There should be an empty line before the table (it is required to get proper output from DocFX).
- Tables should start with the character `|` and ends with the same character.
- Tables are at least 2 columns
- The second line of the table should be with at least 3 characters `-` as separators
