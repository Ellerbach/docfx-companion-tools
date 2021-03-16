# Markdownlint: using a Markdown linter

## What is Markdown

Markdown is a lightweight markup language that you can use to add formatting elements to plaintext text documents. Created by John Gruber in 2004, Markdown is now one of the world’s most popular markup languages.

Using Markdown is different than using a WYSIWYG editor. In an application like Microsoft Word, you click buttons to format words and phrases, and the changes are visible immediately. Markdown isn’t like that. When you create a Markdown-formatted file, you add Markdown syntax to the text to indicate which words and phrases should look different.

You can find more information, a full documentation [here](https://www.markdownguide.org/).

## Why using a linter

Markdown has specific way of being formatted. It is important to respect this formatting otherwise some interpreter which are strict won't display properly the document. The Azure DevOps interpreter forgive a lot of mistakes and always try to present the document properly. But it's far to be the case for all of them. Linter are often use to help developers properly creating document in any language or markup language.

To help developers and anyone who needs to create Markdown, we propose to use [Markedownlint](https://github.com/DavidAnson/markdownlint) which is easy and the most used linter for Markdown documents. [Markdownlint-cli](https://github.com/igorshubovych/markdownlint-cli) is an easy to use cli based out of Markdownlint.

## Rules

A comprehensive list of rules are available [here](https://github.com/DavidAnson/markdownlint/blob/main/doc/Rules.md). We will use a quite strict approach except for the line length rule MD013 which we won't apply.

A configuration file is present in the root directory of the project for your convenience. The file is `.markdownlint.json` and contain:

```json
{
    "MD013": false
}
```

Then simply run the following command:

```bash
markdownlint -f path_to_your_file.md
```

Note that the -f parameter will fix all basics errors and save you some time.

## Using VS Code extensions

There are couple of VS Code extensions to help you in this task as well. We can recommend [Prettier](https://marketplace.visualstudio.com/items?itemName=esbenp.prettier-vscode) which will catch all of them as well.

## Azure DevOps pipeline

Markdownlinter is also part of the Azure DevOps code quality pipeline which will automatically run upon PRs to dev.

TODO: add a link on the pipeline.
