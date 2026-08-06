// <copyright file="Program.cs" company="DocFx Companion Tools">
// Copyright (c) DocFx Companion Tools. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
using System.CommandLine;
using DocAssembler;
using DocAssembler.Actions;
using DocAssembler.Configuration;
using DocAssembler.FileService;
using DocAssembler.Utils;
using Microsoft.Extensions.Logging;

var logLevel = LogLevel.Warning;

// parameters/options
var configFileOption = new Option<FileInfo>("--config")
{
    Description = "The configuration file for the assembled documentation.",
    Required = true,
};

var workingFolderOption = new Option<DirectoryInfo>("--workingfolder")
{
    Description = "The working folder. Default is the current folder.",
};

var outputFolderOption = new Option<DirectoryInfo>("--outfolder")
{
    Description = "Override the output folder for the assembled documentation in the config file.",
};

var cleanupOption = new Option<bool>("--cleanup-output")
{
    Description = "Cleanup the output folder before generating. NOTE: This will delete all folders and files!",
};

var verboseOption = new Option<bool>("--verbose", "-v")
{
    Description = "Show verbose messages of the process.",
};

// construct the root command
var rootCommand = new RootCommand(
    """
    DocAssembler.
    Assemble documentation in the output folder. The tool will also fix links following configuration.
 
    Return values:
    0 - succesfull.
    1 - some warnings, but process could be completed.
    2 - a fatal error occurred.
    """);

rootCommand.Options.Add(workingFolderOption);
rootCommand.Options.Add(configFileOption);
rootCommand.Options.Add(outputFolderOption);
rootCommand.Options.Add(cleanupOption);
rootCommand.Options.Add(verboseOption);

var initCommand = new Command("init", "Intialize a configuration file in the current directory if it doesn't exist yet.");
rootCommand.Subcommands.Add(initCommand);

// handle the execution of the root command
rootCommand.SetAction(async (ParseResult parseResult, CancellationToken cancellationToken) =>
{
    // setup logging
    SetLogLevel(parseResult);

    LogParameters(
        parseResult.GetValue(configFileOption)!,
        parseResult.GetValue(outputFolderOption),
        parseResult.GetValue(workingFolderOption),
        parseResult.GetValue(cleanupOption));

    // execute the generator
    return (int)await AssembleDocumentationAsync(
        parseResult.GetValue(configFileOption)!,
        parseResult.GetValue(outputFolderOption),
        parseResult.GetValue(workingFolderOption),
        parseResult.GetValue(cleanupOption));
});

// handle the execution of the init command
initCommand.SetAction(async (ParseResult parseResult, CancellationToken cancellationToken) =>
{
    // setup logging
    SetLogLevel(parseResult);

    // execute the configuration file initializer
    return (int)await GenerateConfigurationFile();
});

return await rootCommand.Parse(args).InvokeAsync();

// main process for configuration file generation.
async Task<ReturnCode> GenerateConfigurationFile()
{
    // setup services
    ILogger logger = GetLogger();
    IFileService fileService = new FileService();

    try
    {
        // the actual generation of the configuration file
        ConfigInitAction action = new(Environment.CurrentDirectory, fileService, logger);
        ReturnCode ret = await action.RunAsync();

        logger.LogInformation($"Command completed. Return value: {ret}.");
        return ret;
    }
    catch (Exception ex)
    {
        logger.LogCritical(ex.Message);
        return ReturnCode.Error;
    }
}

// main process for assembling documentation.
async Task<ReturnCode> AssembleDocumentationAsync(
    FileInfo configFile,
    DirectoryInfo? outputFolder,
    DirectoryInfo? workingFolder,
    bool cleanup)
{
    // setup services
    ILogger logger = GetLogger();
    IFileService fileService = new FileService();

    try
    {
        ReturnCode ret = ReturnCode.Normal;

        string currentFolder = workingFolder?.FullName ?? Directory.GetCurrentDirectory();

        // CONFIGURATION
        if (!Path.Exists(configFile.FullName))
        {
            // error: not found
            logger.LogCritical($"Configuration file '{configFile}' doesn't exist.");
            return ReturnCode.Error;
        }

        string json = File.ReadAllText(configFile.FullName);
        var config = SerializationUtil.Deserialize<AssembleConfiguration>(json);
        string outputFolderPath = string.Empty;
        if (outputFolder != null)
        {
            // overwrite output folder with given override value
            config.DestinationFolder = outputFolder.FullName;
            outputFolderPath = outputFolder.FullName;
        }
        else
        {
            outputFolderPath = Path.GetFullPath(Path.Combine(currentFolder, config.DestinationFolder));
        }

        // INVENTORY
        InventoryAction inventory = new(currentFolder, config, fileService, logger);
        ret = await inventory.RunAsync();

        if (ret != ReturnCode.Error)
        {
            if (cleanup && Directory.Exists(outputFolderPath))
            {
                // CLEANUP OUTPUT
                Directory.Delete(outputFolderPath, true);
            }

            // ASSEMBLE
            AssembleAction assemble = new(config, inventory.Files, fileService, logger);
            ret = await assemble.RunAsync();
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
    FileInfo configFile,
    DirectoryInfo? outputFolder,
    DirectoryInfo? workingFolder,
    bool cleanup)
{
    ILogger logger = GetLogger();

    logger.LogInformation($"Configuration : {configFile.FullName}");
    if (outputFolder != null)
    {
        logger.LogInformation($"Output  folder: {outputFolder.FullName}");
    }

    if (workingFolder != null)
    {
        logger.LogInformation($"Working folder: {workingFolder.FullName}");
    }

    logger.LogInformation($"Cleanup       : {cleanup}");
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
ILogger GetLogger() => GetLoggerFactory().CreateLogger(nameof(DocAssembler));
