# Guidelines for creating Markdown files

This document explains couple of guidelines to create a proper markdown file we are used to use in various projects. Those rules help maintaining a great quality repository of files and a readable file structure.

Markdown files are text based. If you want to learn about possibilities (headers, lists, tables, code and such) this cheat sheet was always helpful: [Markdown Cheatsheet · adam-p/markdown-here Wiki · GitHub](https://github.com/adam-p/markdown-here/wiki/Markdown-Cheatsheet). There are a number of them.

## Images and attachments

To keep the markdown and images or attachments separated, it's wise to put images and attachments in a separate folder. There are various approaches. One is to have a central storage location in `/docs/.attachments`. Another option is to have an `assets` subfolder in any folder that contains a markdown using images or attachments.

Having a centralized storage location in `/docs/.attachments` aligns with the way Azure DevOps and the WIKI work. So this could be a smart thing to do if you want to use the web interface of ADO to modify content. In all other cases it is recommended to use the subfolder approach.

## Naming conventions

Please to follow those simple rules:

- Try to only use lowercase in your names
- Don't use spaces, use `-`
- Use only ASCII 7 characters from the alphabet and numerical

## Links

It's ok to use external links.

For links on documentation itself, use a relative link to the file. For example if you are in `/docs/userdocs/en/plant-production` and want to reference the document `index.md` which is located in `/docs/userdocs`, the link will be `../../index.md`.

You may also use relative links to the root of a DocFX project. For example [this link](~/userdocs/index.md) references `/docs/userdocs/index.md` from this very page using a link relative to the project root. The link for this scenario would be `~/userdocs/index.md`. This only works in a DocFx project.

> [!NOTE]
> Using a relative link to something in the repository that is not part of the documentation like a source file can be done in combination with the use of the [DocAssembler](~/tools/DocAssembler/README.md) tool. That tool can, with the proper configuration, change the relative link to a web URL, directing the user to the source file in the repo.

## Markdown linter

It's best to use a linter to make sure the markdown is written properly. It should run in a pipeline blocking a PR when there are errors. Check more details [here](markdown-linter.md).

## Practical Tips & Tricks

### Preview in VS Code

When editing markdown in VS Code, there is a preview button on the top right that will open a live preview window on the left. So you can type and see the result at the same time. This is usually a great way to avoid basic mistakes, making sure your images are showing up properly for example.

![VS Code Preview](assets/vscode-markdown-preview.gif)

### Typora as an alternative tool

[Typora](https://typora.io/) is an editor that can be installed locally to help you generate proper markdown in a WYSIWIG interface. You can switch between rendered input and raw input easily. It also has great features like the ability to create table from a copy/paste of a web page. This is, however, a paid tool. For a small fee you can acquire a user license to use to tool on any of your machines.

> [!NOTE]
> Currently Typora doesn't support a markdownlinter in the editor.

### Use a spell checker

There are plenty of spell checker extensions that will help to reduce the numbers of mistakes. Some recommended extensions for VS Code are [Spell Right](https://marketplace.visualstudio.com/items?itemName=ban.spellright) or [Code Spell Checker](https://marketplace.visualstudio.com/items?itemName=streetsidesoftware.code-spell-checker).

### Patterns for enumerations

It can be a bit frustrating to work with enumerations in text. A key point to keep in mind is that enumerations in markdown are made to be grouped. Once you need a lot of text or code blocks in between items, enumerations are not really the best to use. So here are couple of patterns.

```markdown
# This is the one and only main title

## You can have as many title 2 as you want

1. My enumeration starts with 1
  - I have sub bullets which can be enums as well
  - And another one
1. This one will have number 2
1. And this one is 3

1. Now, this one is 1 again as there is a newline between both enumerations
1. And this is 2 again
```

If you are trying to get a large block of text with paragraphs or a block of code in an enumeration, you can use the `Step #` pattern like this:

```markdown
- Step 1: do something

Put some text here or a code block, images, etc.

- Step 2: do the next step

Again, some text, code or image(s) here.

- Step 3: you can continue this pattern
```

### Using Emoji's

It can be a great way to highlight essential content using Emojis. To add them, use a shortcuts (depending on your operational system):

- on Mac: CTRL + CMD + Space
- on Windows: Win + ; (semi-colon) or Win + . (period)

![Emoji](assets/markdown-icons.png)

## Moving files

In general, be careful with moving or renaming files in the documentation hierarchy. It can have an effect on links in other files. Make sure to use the [DocLinkChecker](~/tools/DocLinkChecker/README.md) tool as a gate in your PR pipelines as well to validate all links.
