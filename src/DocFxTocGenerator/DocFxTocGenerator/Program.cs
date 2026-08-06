// <copyright file="Program.cs" company="DocFx Companion Tools">
// Copyright (c) DocFx Companion Tools. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
using System.CommandLine;
using DocFxTocGenerator;
using DocFxTocGenerator.Actions;
using DocFxTocGenerator.FileService;
using DocFxTocGenerator.Index;
using DocFxTocGenerator.TableOfContents;
using DocFxTocGenerator.Utils;
using Microsoft.Extensions.Logging;

var logLevel = LogLevel.Warning;

// parameters/options
var docsFolder = new Option<DirectoryInfo>("--docfolder", "-d")
{
    Description = "The root folder of the documentation.",
    Required = true,
};
var outputFolder = new Option<DirectoryInfo>("--outfolder", "-o")
{
    Description = "The output folder for the generated table of contents file. Default is the documentation folder.",
};
var verboseOption = new Option<bool>("--verbose", "-v")
{
    Description = "Show verbose messages of the process.",
};
var sequenceOption = new Option<bool>("--sequence", "-s")
{
    Description = "Use .order files per folder to define the sequence of files and directories. Format of the file is filename without extension per line.",
};
var overrideOption = new Option<bool>("--override", "-r")
{
    Description = "Use .override files per folder to define title overrides for files and folders. Format of the file is filename without extension or directory name followed by a semi-column followed by the custom title per line.",
};
var ignoreOption = new Option<bool>("--ignore", "-g")
{
    Description = "Use .ignore files per folder to ignore directories. Format of the file is directory name per line.",
};
var indexingOption = new Option<IndexGenerationStrategy>("--indexing")
{
    Description = "When to generated an index.md for a folder.\nNever          - Do not genereate.\nNoDefault      - When no index.md or readme.md found.\nNoDefaultMulti - When no index.md or readme.md found and multiple files.\nEmptyFolders   - For empty folders.\nNotExists      - When no index found.\nNotExistMulti  - When no index and multiple files.",
    DefaultValueFactory = _ => IndexGenerationStrategy.Never,
};
var folderReferenceOption = new Option<TocFolderReferenceStrategy>("--folderRef")
{
    Description = "Strategy for folder-entry references.\nNone        - Never reference anything.\nIndex       - Index.md only if exists.\nIndexReadme - Index.md or readme.md if exists.\nFirst       - First file in folder if any exists.",
    DefaultValueFactory = _ => TocFolderReferenceStrategy.First,
};
var orderingOption = new Option<TocOrderStrategy>("--ordering")
{
    Description = "How to order items in a folder.\nAll          - Folders and files combined.\nFoldersFirst - Folders first, then files.\nFilesFirst   - Files first, then folders.",
    DefaultValueFactory = _ => TocOrderStrategy.All,
};
var multiTocOption = new Option<int>("--multitoc", "-m")
{
    Description = "Indicates how deep in the tree toc files should be generated for those folders. A depth of 0 is the root only (default behavior).",
};
var camelCaseOption = new Option<bool>("--camelCase")
{
    Description = "Use camel casing for titles.",
};

// deprecated options
var deprecatedIndexOption = new Option<bool>("--index", "-i")
{
    Description = "[Deprecated: please use --indexing NoDefault]\nAuto generate a index.md for folders without readme.md or index.md file.",
    Hidden = true,
};

var deprecatedNoIndexWithOneFileOption = new Option<bool>("--notwithone", "-n")
{
    Description = "[Deprecated: please use --indexing NotExistMultipleFiles]\nOnly auto generate index.md when a directory contains multiple files. Used in combination with --index (-i) flag.",
    Hidden = true,
};

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

rootCommand.Options.Add(docsFolder);
rootCommand.Options.Add(outputFolder);

rootCommand.Options.Add(verboseOption);
rootCommand.Options.Add(sequenceOption);
rootCommand.Options.Add(overrideOption);
rootCommand.Options.Add(ignoreOption);
rootCommand.Options.Add(indexingOption);
rootCommand.Options.Add(folderReferenceOption);
rootCommand.Options.Add(orderingOption);
rootCommand.Options.Add(multiTocOption);
rootCommand.Options.Add(camelCaseOption);

// deprecated: replaced by indexing flag
rootCommand.Options.Add(deprecatedIndexOption);
rootCommand.Options.Add(deprecatedNoIndexWithOneFileOption);

// handle the execution of the root command
rootCommand.SetAction(async (ParseResult parseResult, CancellationToken cancellationToken) =>
{
    // setup logging
    SetLogLevel(parseResult);

    LogParameters(
        parseResult.GetValue(docsFolder)?.FullName!,
        parseResult.GetValue(outputFolder)?.FullName ?? parseResult.GetValue(docsFolder)?.FullName!,
        parseResult.GetValue(sequenceOption),
        parseResult.GetValue(overrideOption),
        parseResult.GetValue(ignoreOption),
        parseResult.GetValue(indexingOption),
        parseResult.GetValue(folderReferenceOption),
        parseResult.GetValue(orderingOption),
        parseResult.GetValue(multiTocOption),
        parseResult.GetValue(camelCaseOption),
        parseResult.GetValue(deprecatedIndexOption),
        parseResult.GetValue(deprecatedNoIndexWithOneFileOption));

    // determine generation type. We're processing the deprecated settings here.
    IndexGenerationStrategy indexing = parseResult.GetValue(indexingOption);
    if (parseResult.GetValue(indexingOption) ==
            IndexGenerationStrategy.Never && parseResult.GetValue(deprecatedIndexOption))
    {
        // only use deprecated setting when indexing is not given.
        indexing = parseResult.GetValue(deprecatedNoIndexWithOneFileOption) ?
                                    IndexGenerationStrategy.NotExistMulti : IndexGenerationStrategy.NotExists;
    }

    // execute the generator
    return (int)await GenerateTocAsync(
        parseResult.GetValue(docsFolder)?.FullName!,
        parseResult.GetValue(outputFolder)?.FullName ?? parseResult.GetValue(docsFolder)?.FullName!,
        parseResult.GetValue(sequenceOption),
        parseResult.GetValue(overrideOption),
        parseResult.GetValue(ignoreOption),
        indexing,
        parseResult.GetValue(folderReferenceOption),
        parseResult.GetValue(orderingOption),
        parseResult.GetValue(multiTocOption),
        parseResult.GetValue(camelCaseOption));
});

return await rootCommand.Parse(args).InvokeAsync();

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
    int tocDepth,
    bool camelCasing)
{
    // setup services
    ILogger logger = GetLogger();
    IFileService fileService = new FileService();

    try
    {
        // first, retrieve data for documentation from the files
        ContentInventoryAction retrieval = new(docsFolder, useOrder, useIngore, useOverride, camelCasing, fileService, logger);
        ReturnCode ret = await retrieval.RunAsync();

        if (ret == 0 && retrieval.RootFolder != null)
        {
            // Now validate folder/file structure. Might generate index, depending on setting.
            EnsureIndexAction validation = new(retrieval.RootFolder, indexStrategy, camelCasing, fileService, logger);
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
    bool camelCasing,
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
    logger!.LogInformation($"Camel casing    : {camelCasing}");
}

void SetLogLevel(ParseResult parseResult)
{
    if (parseResult.GetValue(verboseOption))
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
