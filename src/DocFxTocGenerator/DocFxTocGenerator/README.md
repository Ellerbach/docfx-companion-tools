# Table of Contents (TOC) generator for DocFX

This tool allow to generate a yaml compatible `toc.yml` file for DocFX.

## Usage

```text
DocFxTocGenerator [options]

Options:
  -d, --docfolder <docfolder> (REQUIRED)                      The root folder of the documentation.
  -o, --outfolder <outfolder>                                 The output folder for the generated table of contents
                                                              file. Default is the documentation folder.
  -v, --verbose                                               Show verbose messages of the process.
  -s, --sequence                                              Use .order files per folder to define the sequence of
                                                              files and directories. Format of the file is filename
                                                              without extension per line.
  -r, --override                                              Use .override files per folder to define title overrides
                                                              for files. Format of the file is filename without
                                                              extension followed by a semi-column followed by the
                                                              custom title per line.
  -g, --ignore                                                Use .ignore files per folder to ignore directories.
                                                              Format of the file is directory name per line.
  --indexing                                                  When to generated an index.md for a folder.
  <EmptyFolders|Never|NoDefault|NoDefaultMulti|NotExistMulti  Never          - Do not genereate.
  |NotExists>                                                 NoDefault      - When no index.md or readme.md found.
                                                              NoDefaultMulti - When no index.md or readme.md found and
                                                              multiple files.
                                                              EmptyFolders   - For empty folders.
                                                              NotExists      - When no index found.
                                                              NotExistMulti  - When no index and multiple files.
                                                              [default: Never]
  --folderRef <First|Index|IndexReadme|None>                  Strategy for folder-entry references.
                                                              None        - Never reference anything.
                                                              Index       - Index.md only if exists.
                                                              IndexReadme - Index.md or readme.md if exists.
                                                              First       - First file in folder if any exists.
                                                              [default: First]
  --ordering <All|FilesFirst|FoldersFirst>                    How to order items in a folder.
                                                              All          - Folders and files combined.
                                                              FoldersFirst - Folders first, then files.
                                                              FilesFirst   - Files first, then folders. [default: All]
  -m, --multitoc <multitoc>                                   Indicates how deep in the tree toc files should be
                                                              generated for those folders. A depth of 0 is the root
                                                              only (default behavior).
  --version                                                   Show version information
  -?, -h, --help                                              Show help and usage information
```

Return values:
  0 - succesfull.
  1 - some warnings, but process could be completed.
  2 - a fatal error occurred.

## Warnings, errors and verbose

If the tool encounters situations that might need some action, a warning is written to the output. The table of contents is still created. If the tool encounters an error, an error message is written to the output. The table of contents will not be created.

If you want to trace what the tool is doing, use the `-v or --verbose` flag to output all details of processing the files and folders and creating the table of contents.

## Overall process

The overall process of this tool is:

1. Content inventory - retrieve all folders and files (`*.md` and `*swagger.json`) in the given documentation folder. Flags `-s | --sequence`, `-r | --override` and `-g | --ignore` are processed here to read setting files in the hierarchy.
2. Ensure indexing - validate structure with given settings. Depending on the `--indexing` flag automated `index.md` files are added where necessary.
3. Generate the table of contents - generate the `toc.yml` file(s). For folders it can be indicated if they should have a reference into child files using the `--folderRef` flag. Using the `--ordering` flag the ordering of directories and files can be defined. In this step the `-m | --multitoc <multitoc>` flag is evaluated and processed on generation.

### Title of directories and files

For directories the name of the directory is used by default, where the first character is uppercased and special characters (`[`, `]`, `:`, \`,`\`, `{`, `}`, `(`, `)`, `*`, `/`) are removed and `-`, `_` and multiple spaces are replaced by a single space.

For markdown files the first level-1 heading is taken as title. For swagger files the title and version are taken as title. On error the file name without extension is taken and processed the same way as the name of a directory.

The `.override` setting file can be used to override this behavior. See [Defining title overrides with `.override`](#defining_title_overrides_with__override).

## Folder settings

Folder settings can be provided on ordering directories and files, ignore directories and override titles of files.  Flags `-s | --sequence`, `-r | --override` and `-g | --ignore` are processed here to read setting files in the hierarchy.

### Defining the order with `.order`

If the `-s | --sequence` parameter is provided, the tool will inspect folders if a `.order` file exists and use that to determine the order of files and directories. The `.order` file is just a list of file- and/or directory-names, *case-sensitive* without file extensions. Also see the [Azure DevOps WIKI documentation on this file](https://docs.microsoft.com/en-us/azure/devops/project/wiki/wiki-file-structure?view=azure-devops#order-file).

A sample `.order` file looks like this:

```text
getting-started
working-agreements
developer
```

Ordering of directories and files in a folder is influenced by the `-s | --sequence` flag in combination with the `.order` file in that directory, combined with the (optional) `--ordering` flag. Also see [Ordering](#ordering).

### Defining directories to ignore with `.ignore`

If the `-g | --ignore` parameter is provided, the tool will inspect folders if a `.ignore` file exists and use that to ignore directories. The `.ignore` file is just a list of file- and/or directory-names, *case-sensitive* without file extensions.

A sample `.ignore` file looks like this:

```text
node_modules
bin
```

It only applies to the folder it's in, not for other subfolders under that folder.

### Defining title overrides with `.override`

If the `-r | --override` parameter is provided, the tool will inspect folders if a `.override` file exists and use that for overrides of file titles as they will show in the generated `toc.yml`. The `.override` file is a list of file- and/or directory-names, *case-sensitive* without file extensions, followed by a semi-column, followed by the title to use.

For example, if the folder name is `introduction`, the default behavior will be to create the name `Introduction`. If you want to call it `To start with`, you can use overrides, like in the following example:

```text
introduction;To start with
working-agreements;All working agreements of all teams
```

If there are files or directories which are not in the .order file, they will be alphabetically ordered on the title and added after the ordered entries. The title for an MD-file is taken from the H1-header in the file. The title for a directory is the directory-name, but cleanup from special characters and the first character in capitals.

## Automatic generating `index.md` files

If the `-indexing <method>` parameter is provided the `method` defines the conditions for generating an `index.md` file. The options are:

* `Never` - never generate an `index.md`. This is the default.
* `NoDefault` - generate an `index.md` when no `index.md` or `readme.md` is found in a folder.
* `NoDefaultMulti` - generate an `index.md` when no `index.md` or `readme.md` is found in a folder and there are 2 or more files.
* `NotExists` - generate an `index.md` when no `index.md` file is found in a folder.
* `NotExistsMulti` - generate an `index.md` when no `index.md` file is found in a folder and there are 2 or more files.
* `EmptyFolders` - generate an `index.md` when a folder doesn't contain any files.

### Template for generating an `index.md`

When an `index.md` file is generated, this is done by using a [Liquid template](https://shopify.github.io/liquid/). The tool contains a *default template*:

```liquid
# {{ current.DisplayName }}

{% comment -%}Looping through all the files and show the display name.{%- endcomment -%}
{% for file in current.Files -%}
{%- if file.IsMarkdown -%}
* [{{ file.DisplayName }}]({{ file.Name }})
{% endif -%}
{%- endfor %}
```

This results in a markdown file like this:

```markdown
# Brasil

* [Nova Friburgo](nova-friburgo.md)
* [Rio de Janeiro](rio-de-janeiro.md)
* [Sao Paulo](sao-paulo.md)
```

You can also provide a customized template to be used. The ensure indexing process will look for a file with the name `.index.liquid` in the folder where an `index.md` needs to be generated. If it doesn't exist in that folder it's traversing all parent folders up to the root and until a `.index.liquid` file is found.

In the template access is provided to this information:

* `current` - this is the current folder that needs an `index.md` file of type `FolderData`.
* `root` - this is the root folder of the complete hierarchy of the documentation of type `FolderData`.

#### `FolderData` class

| Property       | Description                                                  |
| -------------- | ------------------------------------------------------------ |
| `Name`         | Folder name from disk                                        |
| `DisplayName`  | Title of the folder                                          |
| `Path`         | Full path of the folder                                      |
| `Sequence`     | Sequence number from the `.order` file or `int.MaxValue` when not defined. |
| `RelativePath` | Relative path of the folder from the root of the documentation. |
| `Parent`       | Parent folder. When `null` it's the root folder.             |
| `Folders`      | A list of `FolderData` objects for the sub-folders in this folder. |
| `Files`        | A list of `FileData` objects for the files in this folder.   |
| `HasIndex`     | A `boolean` indicating whether this folder contains an `index.md` |
| `Index`        | The `FileData` object of the `index.md` in this folder if it exists. If it doesn't exists this will be `null`. |
| `HasReadme`    | A `boolean` indicating whether this folder contains an `README.md` |
| `Readme`       | The `FileData` object of the `README.md` in this folder if it exists. If it doesn't exists this will be `null`. |

#### `FileData` class

| Property       | Description                                                  |
| -------------- | ------------------------------------------------------------ |
| `Name`         | Filename including the extension                             |
| `DisplayName`  | Title of the file.                                           |
| `Path`         | Full path of the file                                        |
| `Sequence`     | Sequence number from the `.order` file or `int.MaxValue` when not defined. |
| `RelativePath` | Relative path of the file from the root of the documentation. |
| `Parent`       | Parent folder.                                               |
| `IsMarkdown`   | A `boolean` indicating whether this file is a markdown file. |
| `IsSwagger`    | A `boolean` indicating whether this file is a Swagger JSON file. |
| `IsIndex`      | A `boolean` indicating whether this file is an `index.md` file. |
| `IsReadme`     | A `boolean` indicating whether this file is a `README.md` file. |

For more information on how to use Liquid logic, see the article [Using Liquid for text-based templates with .NET | by Martin Tirion | Medium](https://mtirion.medium.com/using-liquid-for-text-base-templates-with-net-80ae503fa635) and the [Liquid reference](https://shopify.github.io/liquid/basics/introduction/).

Liquid, by design, is very forgiving. If you reference an object or property that doesn't exist, it will render to an empty string. But if you introduce language errors (missing `{{` for instance) an error is thrown, the error is in the output of the tool but will not crash the tool, but will be resulting in error code 1 (warning). In the case of an error like this, no `index.md` is generated.

## Ordering

There are these options for ordering directories and folders:

* `All` - order all directories and files by sequence, then by title.
* `FoldersFirst` - order all directories first, then the files. Ordering is for each of them done by sequence, then by title.
* `FilesFirst` -  order all files first, then the folders. Ordering is for each of them done by sequence, then by title.

For all of these options the `.order` file can be used when it exists and the `-s | --sequence` flag is used. The line in the `.order` file determines the sequence of a file or directory. So, the first entry results in sequence 1. In all other cases a folder or file has an equal sequence of `int.MaxValue`.

By default the ordering of files is applied where the `index.md` is first and the `README.md` is second, optionally followed by the settings from the `.order` file. This behavior can only be overruled by adding `index` and/or `readme` to a `.order` file and use of the `-s | --sequence` flag.

> [!NOTE]
>
> `README` and `index` are always validated **case-sensitive** to make sure they are ordered correctly. All other file names and directory names are matched **case-insensitive**.

## Folder referencing

The table of content is constructed from the folders and files. For folders there are various strategies to determine if it will have a reference:

* `None` - no reference for all folders.
* `Index` - reference the `index.md` in the folder if it exists.
* `IndexReadme` - reference the `index.md` if it exists, otherwise reference the `README.md` if it exists.
* `First` - reference the first file in the folder after [ordering](#ordering) has been applied.

When using DocFx to generate the website, folders with no reference will just be entries in the hive that can be opened and closed. The UI will determine what will be showed as content.

## Multiple table of content files

The default for this tool is to generate only one `toc.yml` file in the root of the output directory. But with a large hierarchy, this file can get pretty large. In that case it might be easier to have a few `toc.yml` files per level to have multiple, smaller `toc.yml` files.

The `-m | --multitoc` option will control how far down the hierarchy `toc.yml` files are generated. Let's explain this feature by an example hierarchy:

```text
📂docs
  📄README.md
  📂continents
    📄index.md
  	📂americas
  	  📄README.md
  	  📄extra-facts.md
  	  📂brasil
  	    📄README.md
  	    📄nova-friburgo.md
  	    📄rio-de-janeiro.md
  	  📂united-states
        📄los-angeles.md
        📄new-york.md
        📄washington.md
    📂europe
  	  📄README.md
  	  📂germany
  	    📄berlin.md
  	    📄munich.md
      📂netherlands
        📄amsterdam.md
        📄rotterdam.md
  📂vehicles
    📄index.md
  	📂cars
  	  📄README.md
  	  📄audi.md
  	  📄bmw.md  			
```

### Default behavior or depth=0

By default, when the `depth` is `0` (or the option is omitted), only one `toc.yml` file is generated in the root of the output folder containing the complete hierarchy of folders and files. For the example hierarchy it would look like this:

```yaml
# This is an automatically generated file
- name: Multi toc example
  href: README.md
- name: Continents
  href: continents/index.md
  items:
  - name: Americas
    href: continents/americas/README.md
    items:
    - name: Americas Extra Facts
      href: continents/americas/extra-facts.md
    - name: Brasil
      href: continents/americas/brasil/README.md
      items:
      - name: Nova Friburgo
        href: continents/americas/brasil/nova-friburgo.md
      - name: Rio de Janeiro
        href: continents/americas/brasil/rio-de-janeiro.md
    - name: Los Angeles
      href: continents/americas/united-states/los-angeles.md
      items:
      - name: New York
        href: continents/americas/united-states/new-york.md
      - name: Washington
        href: continents/americas/united-states/washington.md
  - name: Europe
    href: continents/europe/README.md
    items:
    - name: Amsterdam
      href: continents/europe/netherlands/amsterdam.md
      items:
      - name: Rotterdam
        href: continents/europe/netherlands/rotterdam.md
    - name: Berlin
      href: continents/europe/germany/berlin.md
      items:
      - name: Munich
        href: continents/europe/germany/munich.md
- name: Vehicles
  href: vehicles/index.md
  items:
  - name: Cars
    href: vehicles/cars/README.md
    items:
    - name: Audi
      href: vehicles/cars/audi.md
    - name: BMW
      href: vehicles/cars/bmw.md

```

### Behavior with depth=1 or more

When a `depth` of `1` is given, a `toc.yml` is generated in the root of the output folder and in each sub-folder of the documentation root. The `toc.yml` in the root will only contain documents of the folder itself and references to the `toc.yml` files in the sub-folders. In our example for the root it would look like this:

```yaml
# This is an automatically generated file
- name: Multi toc example
  href: README.md
- name: Continents
  href: continents/toc.yml
- name: Vehicles
  href: vehicles/toc.yml
```

The `toc.yml` files in the sub-folders `continents` and `vehicles` will contain the complete hierarchy from that point on. For instance, for `vehicles` it will look like this:

```yaml
# This is an automatically generated file
- name: Cars
  href: cars/README.md
  items:
  - name: Audi
    href: cars/audi.md
  - name: BMW
    href: cars/bmw.md
```

