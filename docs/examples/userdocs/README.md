# Guidelines for end user documentation

The end user documentation is placed into the directory `/docs/userdocs` and is following the language directory pattern.

## Advantages

this pattern can be found into most of the online website like the Microsoft docs website. This has multiple advantages like having a clean structure with a clean navigation per language and allowing an easy linking to the resources.

Creating a new language is made easy by copying/pasting the structure with all the files and then localizing.

## Inconvenient

The main inconvenient in this choice is the fact you need to make sure the structure per language is the same otherwise the deep links from the application side may be broken.

To mitigate this inconvenient, a structure checker should be built giving warnings if the structure is not the same for all the languages.

## Folder structure

This is how the directory should be organized from the root folder:

```text
📁userdocs
  📁assets
    📄en.jpg
    📄de.png
  📁en
    📄index.md
    📁page-examples
      📄README.md
      📄second-file.md
  📁de
    📄index.md
    📁page-examples
      📄README.md
      📄second-file.md
  index.md
```

In this example, the names are random, you'll have of course to adjust to the names you want.

## Case of attachments

Note that the `assets` folder is common for all the languages. This allows flexibility to reuse documents, pictures without duplicating them.

In case you need a picture in multiple languages them, you should use a suffix for the language. For example, you need to take a screen capture of the user interface in English and another one in German. You can name the files like `picture-en.jpg` for English and `picture-de.jpg` for German. This will allow you to easily find them in the attachment folder as well as making it easy for localization. You can also have language specific files in an `assets` subfolder.

## Onboarding a new language

To onboard a new language, copy from the main language the content of the language folder. Let's say the main language is German and you want to add French. Copy all what is in the main `/userdocs/de` to a new folder `/userdocs/fr`. You then can focus on localization.

## Folder name and special name override

You can use a file named .override to override the name of files or directory. Example:

```text
pages-example;Beispielseiten
```

In this case, if placed in the `/userdocs/de` folder, it will override the name `pages-example` with `Beispielseiten`.
