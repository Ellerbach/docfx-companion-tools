# Using a markdown linter

## What is markdown

Markdown is a lightweight markup language that you can use to add formatting elements to plaintext text documents. Created by John Gruber in 2004, markdown is now one of the world’s most popular markup languages.

Using Markdown is different than using a WYSIWYG editor. In an application like Microsoft Word, you click buttons to format words and phrases, and the changes are visible immediately. Markdown isn’t like that. When you create a markdown-formatted file, you add markdown syntax to the text to indicate which words and phrases should look different.

You can find more information, a full documentation [here](https://www.markdownguide.org/).

## Using a linter

Markdown has specific way of being formatted. It is important to respect this formatting otherwise some interpreter which are strict won't display properly the document. A linter can help developers properly format documents.

To help to write valid markdown, you can use [Markedownlint](https://github.com/DavidAnson/markdownlint) which is easy and the most used linter for markdown documents. [Markdownlint-cli](https://github.com/igorshubovych/markdownlint-cli) is an easy to use markdownlint on the command line.

## Rules

A comprehensive list of rules are available [here](https://github.com/DavidAnson/markdownlint/blob/main/doc/Rules.md). Also because of the level of support, it is recommended to use a strict approach. We recommend to exclude rule [MD013 - Line length](https://github.com/DavidAnson/markdownlint/blob/main/doc/md013.md), as it can cause to make a markdown file harder to read and maintain.

A configuration file can be placed in the root of the repository to make it available throughout all markdown files in that repo. The file is `.markdownlint.json`. This is an example:

```json
{
    "MD013": false
}
```

Then simply run the following command using the `mardownlint-cli` tool:

```bash
markdownlint path_to_your_file.md
```

or to scan all files:

```bash
markdownlint **/*.md
```

## Using VS Code extensions

There are VS Code extensions to help you write proper markdown. We can recommend [markdownlint extension for VSCode](https://marketplace.visualstudio.com/items?itemName=DavidAnson.vscode-markdownlint) and [Prettier](https://marketplace.visualstudio.com/items?itemName=esbenp.prettier-vscode).
