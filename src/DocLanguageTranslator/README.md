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

All the Markdown's file names **must** be the same in all the sub directories. This tool can check the integrity as well as automatically creating the missing files and translate them at the same time.

## Usage

```text
DocLanguageTranslator -d <docs folder> [-k <key>] [-l <location>] [-cv]

-d, --docfolder       Required. Folder containing the documents.
-v, --verbose         Show verbose messages.
-k, --key             The Azure Cognitive Services key to use.
-l, --location        The Azure Cognitive Services location to use. Default location is westeurope if nothing is provided.
-c, --check           Check the integrity of the file structure.
--help                Display this help screen.
--version             Display version information.
```

If the `-c or --check` is not provided, having a key is mandatory.

If normal return code of the tool is 0, but on error it returns 1.

## Warnings, errors and verbose

If the tool encounters situations that might need some action, a warning is written to the output. The table of contents is still created.

If the tool encounters an error, an error message is written to the output. The table of contents will not be created. The tool will return errorcode 1.

If you want to trace what the tool is doing, use the `-v or verbose` flag to output all details of processing the files and folders and creating the table of contents.

## Checking file structure integrity

If the `-c or --check` parameter is provided, the tool will inspect every folder with Markdown file and will check that those files are present in all the other language folder.

If there are not the exact same Markdown files (extension must be `.md`) an error will be raised and the missing files will be displayed in the output.

## Creating the missing files for all language directories

If the `-k or --key` parameter is mandatory for the tool to create the missing pages. Let's take the following structure example:

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
* The directory can be absolute (e.g. `c:\path\userdocs`) or relative (e.g. `.\userdocs`) and has to be the root directory where the language folders are so `en`, `de`and `fr` in this case (e.g. the folder fill be then `\usersdocs\en`, `userdocs\de`, `userdocs\fr`).

Once you'll run the command, the program will look at the exiting file in each directory and will translate them and place them in the correct destination folder. So after the tool will run, you will find:

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

The full file structure and all the Markdown files fill be created in the `fr` directory and translated to French from the different sources.

The `and-more.md` file existing only in the `de` language will be translated to English and French.

### Limitations

* The translation process is not perfect! It is more than strongly encouraged to have someone speaking natively the language to help in doing the translation in a better way.
* Please check all the relative links, including the images, sometimes, they are translated while they should not be. Use the VS Code preview to make sure all images are displayed correctly.
* The translation is taking some time as it's done per paragraph and a request is done for every paragraph. This allow a smooth process and reducing the cost of a fast translation.
* Please make sure to run the linter after any translation, the translation may have created additional line ending, changed some tables, etc.
* If you hit a limit, the tool will wait and will try again.
