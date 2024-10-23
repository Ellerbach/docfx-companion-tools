// <copyright file="Program.cs" company="DocFx Companion Tools">
// Copyright (c) DocFx Companion Tools. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using DocFxTocGenerator;
using DocFxTocGenerator.Actions;
using DocFxTocGenerator.FileService;
using DocFxTocGenerator.Index;
using DocFxTocGenerator.TableOfContents;
using DocFxTocGenerator.Utils;
using Microsoft.Extensions.Logging;

var logLevel = LogLevel.Warning;

// parameters/options
var docsFolder = new Option<DirectoryInfo>(
    name: "--docfolder",
    description: "The root folder of the documentation.");
docsFolder.IsRequired = true;
docsFolder.AddAlias("-d");
var outputFolder = new Option<DirectoryInfo>(
    name: "--outfolder",
    description: "The output folder for the generated table of contents file. Default is the documentation folder.");
outputFolder.AddAlias("-o");
var verboseOption = new Option<bool>(
    name: "--verbose",
    description: "Show verbose messages of the process.");
verboseOption.AddAlias("-v");
var sequenceOption = new Option<bool>(
    name: "--sequence",
    description: "Use .order files per folder to define the sequence of files and directories. Format of the file is filename without extension per line.");
sequenceOption.AddAlias("-s");
var overrideOption = new Option<bool>(
    name: "--override",
    description: "Use .override files per folder to define title overrides for files. Format of the file is filename without extension followed by a semi-column followed by the custom title per line.");
overrideOption.AddAlias("-r");
var ignoreOption = new Option<bool>(
    name: "--ignore",
    description: "Use .ignore files per folder to ignore directories. Format of the file is directory name per line.");
ignoreOption.AddAlias("-g");
var indexingOption = new Option<IndexGenerationStrategy>(
    name: "--indexing",
    description: "When to generated an index.md for a folder.\nNever          - Do not genereate.\nNoDefault      - When no index.md or readme.md found.\nNoDefaultMulti - When no index.md or readme.md found and multiple files.\nEmptyFolders   - For empty folders.\nNotExists      - When no index found.\nNotExistMulti  - When no index and multiple files.");
indexingOption.SetDefaultValue(IndexGenerationStrategy.Never);
var folderReferenceOption = new Option<TocFolderReferenceStrategy>(
    name: "--folderRef",
    description: "Strategy for folder-entry references.\nNone        - Never reference anything.\nIndex       - Index.md only if exists.\nIndexReadme - Index.md or readme.md if exists.\nFirst       - First file in folder if any exists.");
folderReferenceOption.SetDefaultValue(TocFolderReferenceStrategy.First);
var orderingOption = new Option<TocOrderStrategy>(
    name: "--ordering",
    description: "How to order items in a folder.\nAll          - Folders and files combined.\nFoldersFirst - Folders first, then files.\nFilesFirst   - Files first, then folders.");
orderingOption.SetDefaultValue(TocOrderStrategy.All);
var multiTocOption = new Option<int>(
    name: "--multitoc",
    description: "Indicates how deep in the tree toc files should be generated for those folders. A depth of 0 is the root only (default behavior).");
multiTocOption.AddAlias("-m");

// deprecated options
var deprecatedIndexOption = new Option<bool>(
    name: "--index",
    description: "[Deprecated: please use --indexing NoDefault]\nAuto generate a index.md for folders without readme.md or index.md file.");
deprecatedIndexOption.IsHidden = true;
deprecatedIndexOption.AddAlias("-i");

var deprecatedNoIndexWithOneFileOption = new Option<bool>(
    name: "--notwithone",
    description: "[Deprecated: please use --indexing NotExistMultipleFiles]\nOnly auto generate index.md when a directory contains multiple files. Used in combination with --index (-i) flag.");
deprecatedNoIndexWithOneFileOption.IsHidden = true;
deprecatedNoIndexWithOneFileOption.AddAlias("-n");

// construct the root command
var rootCommand = new RootCommand(
    """
    DocFxTocGenerator.
    Generate table of contents for documentation. The tool scans for *.md files and *swagger.json files.
 
    Return values:
    0 - succesfull.
    1 - some warnings, but process could be completed.
    2 - a fatal error occurred.
    """);

rootCommand.AddOption(docsFolder);
rootCommand.AddOption(outputFolder);

rootCommand.AddOption(verboseOption);
rootCommand.AddOption(sequenceOption);
rootCommand.AddOption(overrideOption);
rootCommand.AddOption(ignoreOption);
rootCommand.AddOption(indexingOption);
rootCommand.AddOption(folderReferenceOption);
rootCommand.AddOption(orderingOption);
rootCommand.AddOption(multiTocOption);

// deprecated: replaced by indexing flag
rootCommand.AddOption(deprecatedIndexOption);
rootCommand.AddOption(deprecatedNoIndexWithOneFileOption);

// handle the execution of the root command
rootCommand.SetHandler(async (context) =>
{
    // setup logging
    SetLogLevel(context);

    LogParameters(
        context.ParseResult.GetValueForOption(docsFolder)?.FullName!,
        context.ParseResult.GetValueForOption(outputFolder)?.FullName ?? context.ParseResult.GetValueForOption(docsFolder)?.FullName!,
        context.ParseResult.GetValueForOption(sequenceOption),
        context.ParseResult.GetValueForOption(overrideOption),
        context.ParseResult.GetValueForOption(ignoreOption),
        context.ParseResult.GetValueForOption(indexingOption),
        context.ParseResult.GetValueForOption(folderReferenceOption),
        context.ParseResult.GetValueForOption(orderingOption),
        context.ParseResult.GetValueForOption(multiTocOption),
        context.ParseResult.GetValueForOption(deprecatedIndexOption),
        context.ParseResult.GetValueForOption(deprecatedNoIndexWithOneFileOption));

    // determine generation type. We're processing the deprecated settings here.
    IndexGenerationStrategy indexing = context.ParseResult.GetValueForOption(indexingOption);
    if (context.ParseResult.GetValueForOption(indexingOption) ==
            IndexGenerationStrategy.Never && context.ParseResult.GetValueForOption(deprecatedIndexOption))
    {
        // only use deprecated setting when indexing is not given.
        indexing = context.ParseResult.GetValueForOption(deprecatedNoIndexWithOneFileOption) ?
                                    IndexGenerationStrategy.NotExistMulti : IndexGenerationStrategy.NotExists;
    }

    // execute the generator
    context.ExitCode = (int)await GenerateTocAsync(
        context.ParseResult.GetValueForOption(docsFolder)?.FullName!,
        context.ParseResult.GetValueForOption(outputFolder)?.FullName ?? context.ParseResult.GetValueForOption(docsFolder)?.FullName!,
        context.ParseResult.GetValueForOption(sequenceOption),
        context.ParseResult.GetValueForOption(overrideOption),
        context.ParseResult.GetValueForOption(ignoreOption),
        indexing,
        context.ParseResult.GetValueForOption(folderReferenceOption),
        context.ParseResult.GetValueForOption(orderingOption),
        context.ParseResult.GetValueForOption(multiTocOption));
});

return await rootCommand.InvokeAsync(args);

// main process for TOC generation.
async Task<ReturnCode> GenerateTocAsync(
    string docsFolder,
    string outputFolder,
    bool useOrder,
    bool useOverride,
    bool useIngore,
    IndexGenerationStrategy indexStrategy,
    TocFolderReferenceStrategy folderReferenceStrategy,
    TocOrderStrategy orderStrategy,
    int tocDepth)
{
    // setup services
    ILogger logger = GetLogger();
    IFileService fileService = new FileService();

    try
    {
        // first, retrieve data for documentation from the files
        ContentInventoryAction retrieval = new(docsFolder, useOrder, useIngore, useOverride, fileService, logger);
        ReturnCode ret = await retrieval.RunAsync();

        if (ret == 0 && retrieval.RootFolder != null)
        {
            // Now validate folder/file structure. Might generate index, depending on setting.
            EnsureIndexAction validation = new(retrieval.RootFolder, indexStrategy, fileService, logger);
            ret = await validation.RunAsync();

            if (ret == 0)
            {
                // the actual generation of the table of contents
                GenerateTocAction generation = new(
                    outputFolder,
                    retrieval.RootFolder,
                    folderReferenceStrategy,
                    orderStrategy,
                    tocDepth,
                    fileService,
                    logger);
                ret = await generation.RunAsync();
            }
        }

        logger.LogInformation($"Command completed. Return value: {ret}.");
        return ret;
    }
    catch (Exception ex)
    {
        logger.LogCritical(ex.Message);
        return ReturnCode.Error;
    }
}

// output logging of parameters
void LogParameters(
    string docsFolder,
    string outputFolder,
    bool useOrder,
    bool useOverride,
    bool useIngore,
    IndexGenerationStrategy indexStrategy,
    TocFolderReferenceStrategy folderReferenceStrategy,
    TocOrderStrategy orderStrategy,
    int tocDepth,
    bool generateIndex,
    bool noIndexWithOneFile)
{
    ILogger logger = GetLogger();

    logger!.LogInformation($"Documents       : {docsFolder}");
    logger!.LogInformation($"Output          : {outputFolder}");
    logger!.LogInformation($"Use .order      : {useOrder}");
    logger!.LogInformation($"Use .override   : {useOverride}");
    logger!.LogInformation($"Use .ignore     : {useIngore}");

    // obsolete
    IndexGenerationStrategy logIndexStrategy = indexStrategy;
    if (indexStrategy == IndexGenerationStrategy.Never && generateIndex)
    {
        logger!.LogWarning($"*** You are using deprecated parameters --index|-i and/or --notwithone|-n.\nPlease change to the use of --indexing.");

        // only use obsolete setting when indexStrategy is not given.
        logIndexStrategy = noIndexWithOneFile ? IndexGenerationStrategy.NotExistMulti : IndexGenerationStrategy.NotExists;
    }

    logger!.LogInformation($"Index strategy  : {logIndexStrategy}");
    logger!.LogInformation($"Folder reference: {folderReferenceStrategy}");

    logger!.LogInformation($"Order strategy  : {orderStrategy}");
    logger!.LogInformation($"TOC depth       : {tocDepth}{(tocDepth > 0 ? string.Empty : " (1 TOC hierarchy)")}");
}

void SetLogLevel(InvocationContext context)
{
    if (context.ParseResult.GetValueForOption(verboseOption))
    {
        logLevel = LogLevel.Debug;
    }
    else
    {
        logLevel = LogLevel.Warning;
    }
}

ILoggerFactory GetLoggerFactory() => LogUtil.GetLoggerFactory(logLevel);
ILogger GetLogger() => GetLoggerFactory().CreateLogger(nameof(DocFxTocGenerator));
