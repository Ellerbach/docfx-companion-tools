# Translate documentation pages for DocFX

This tool allows to generate and translate automatically missing files or identify missing files in multi language pattern directories. The pattern that should be followed by end user documentation is the following:

```text
/userdocs
  /.attachments
    picture-en.jpg
    picture-de.jpg
    photo.pgn
    otherdoc.pptx
  /en
    index.md
    /plant-production
      morefiles.md
      and-more.md
  /de
    .override
    index.md
    /plant-production
      morefiles.md
      and-more.md
  /zh-Hans
    .override
    index.md
    /plant-production
      morefiles.md
      and-more.md
  index.md
  toc.yml
```

As for the rest of the documentation, all attachments should be in the `.attachments` folder and all sub folder directories should match a language international code. In the previous example, `en` for English, `de` for German, and `zh-Hans` for Chinese (Simplified).

All translatable file names (Markdown `.md` and YAML `.yml`) **must** be the same in all the sub directories. This tool can check the integrity as well as automatically creating the missing files and translate them at the same time.

## Usage

```text
DocLanguageTranslator [options]

Options:
  -d, --docfolder <docfolder> (REQUIRED)  Folder containing the documents.
  -v, --verbose                           Show verbose messages. [default: False]
  -k, --key <key>                         The translator Azure Cognitive Services key.
  -l, --location <location>               The translator Azure Cognitive Services location. [default: westeurope]
  -s, --source <language>                 The source language of files to use for missing translations.
  -c, --check                             Check missing files in structure only. [default: False]
  -f, --sourcefile <sourcefile>           The source file path for line range translation.
  -r, --lines <lines>                     The range of lines to translate (e.g., '1-10'). Requires --sourcefile.
  --version                               Show version information
  -?, -h, --help                          Show help and usage information
```

If the `-c or --check` is not provided, having a key is mandatory.

If normal return code of the tool is 0, but on error it returns 1.

## Warnings, errors and verbose

If the tool encounters situations that might need some action, a warning is written to the output. The table of contents is still created.

If the tool encounters an error, an error message is written to the output. The table of contents will not be created. The tool will return errorcode 1.

If you want to trace what the tool is doing, use the `-v or verbose` flag to output all details of processing the files and folders and creating the table of contents.

## Checking file structure integrity

If the `-c or --check` parameter is provided, the tool will inspect every folder with translatable files and will check that those files are present in all the other language folder.

If there are not the exact same files (supported extensions: Markdown `.md` and YAML `.yml`) an error will be raised and the missing files will be displayed in the output.

## Creating the missing files for all language directories

The `-k or --key` parameter is mandatory for the tool to create the missing pages. Let's take the following structure example:

```text
/userdocs
  /.attachments
    picture-en.jpg
    picture-de.jpg
    photo.pgn
    otherdoc.pptx
  /en
    index.md
    /plant-production
      morefiles.md
  /de
    .override
    index.md
    one-more.md
    /plant-production
      morefiles.md
      and-more.md
  /fr
  index.md
  toc.yml
```

You will have to run the tool with a command line like: `DocLanguageTranslator -d c:\path\userdocs -k abcdef0123456789abcdef0123456789`

*Notes*:

* Your key has to be a valid key. This is an example key.
* The directory can be absolute (e.g. `c:\path\userdocs`) or relative (e.g. `.\userdocs`) and has to be the root directory where the language folders are so `en`, `de` and `fr` in this case (e.g. the folders will then be `\usersdocs\en`, `userdocs\de`, and `userdocs\fr`, respectively).

Once you run the command, the program will look at the exiting file in each directory and will translate them and place them in the correct destination folder. So after the tool will run, you will find:

```text
/userdocs
  /.attachments
    picture-en.jpg
    picture-de.jpg
    photo.pgn
    otherdoc.pptx
  /en
    index.md
    one-more.md
    /plant-production
      morefiles.md
      and-more.md
  /de
    .override
    index.md
    one-more.md
    /plant-production
      morefiles.md
      and-more.md
  /fr
    index.md
    one-more.md
    /plant-production
      morefiles.md
      and-more.md
  index.md
  toc.yml
```

The full file structure and all translatable files (`.md` and `.yml`) will be created in the `fr` directory and translated to French from the different sources.

Markdown files (`.md`) are translated using a Markdown-aware pipeline that preserves document structure and links. All other supported files (`.yml`) are translated as plain text.

The `and-more.md` file existing only in the `de` language will be translated to English and French.

### Source language

In the previous example, "en" is automatically selected as the source language to translate most of the missing files, because it's the first folder, listed alphabetically, which contains the source files.

To explicitly set the source language, pass the language code to the `-s or --source` command line option.

In this case, only missing files which exist in the source language directory will be used for translations.

### Translating specific line ranges

If you only need to translate a specific range of lines from a source document (instead of the entire file), you can use the `-r or --lines` option together with the `-f or --sourcefile` option.
This is useful when you've updated a specific section of a document and want to apply that change to all translated versions without re-translating the entire file.

**Example:**

```bash
DocLanguageTranslator -d c:\path\userdocs -k your-key -f c:\path\userdocs\en\file1.md -r 10-25
```

This command will:

1. Read lines 10-25 from `c:\path\userdocs\en\file1.md`
2. Translate those lines to all other language directories (e.g., `de`, `fr`, `zh-Hans`)
3. Replace lines 10-25 in the corresponding target files with the translated content

**Requirements:**

* The `--sourcefile` option is required when using `--lines`
* The source file must exist within the documentation folder
* Target files must already exist in other language directories
* The line range format must be `start-end` (e.g., `1-10`, `5-20`)
* Line numbers are 1-based and inclusive

**Notes:**

* If a target file doesn't exist, a warning will be displayed and that language will be skipped
* Use the `--check` option with `--lines` to verify that target files exist without translating
* When using `--check` with `--lines`, verbose output (`-v`) will display which files would be translated and show the specific lines that would be translated

**Check-only mode example:**

```bash
DocLanguageTranslator -d c:\path\userdocs -f c:\path\userdocs\en\file1.md -r 10-25 --check -v
```

This will verify that target files exist in all language directories without performing any translation. With verbose mode enabled, it will also display:

* Which target files would receive the translated content
* The source and target languages for each translation
* The actual content of the lines that would be translated

> **Design note:** The explicit line range approach was chosen over automatic change detection (e.g., diff-based or marker-based) because it is simpler, fully predictable, and does not depend on version control state or special syntax in documents. The trade-off is that users must identify the line numbers themselves, but this keeps the tool deterministic and free of hidden heuristics.

### Limitations

* The translation process is not perfect! It is more than strongly encouraged to have someone speaking natively the language to help in doing the translation in a better way.
* Please check all the relative links, including the images, sometimes, they are translated while they should not be. Use the VS Code preview to make sure all images are displayed correctly.
* The translation is taking some time as it's done per paragraph and a request is done for every paragraph. This allow a smooth process and reducing the cost of a fast translation.
* Please make sure to run the linter after any translation, the translation may have created additional line ending, changed some tables, etc.
* If you hit a limit, the tool will wait and will try again.
