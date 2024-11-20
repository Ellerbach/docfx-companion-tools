// <copyright file="Program.cs" company="DocFx Companion Tools">
// Copyright (c) DocFx Companion Tools. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using DocAssembler;
using DocAssembler.Actions;
using DocAssembler.FileService;
using DocAssembler.Utils;
using Microsoft.Extensions.Logging;

var logLevel = LogLevel.Warning;

// parameters/options
var configFileOption = new Option<FileInfo>(
    name: "--config",
    description: "The configuration file for the assembled documentation.")
{
    IsRequired = true,
};

var workingFolderOption = new Option<DirectoryInfo>(
    name: "--workingfolder",
    description: "The working folder. Default is the current folder.");

var outputFolderOption = new Option<DirectoryInfo>(
    name: "--outfolder",
    description: "Override the output folder for the assembled documentation in the config file.");

var verboseOption = new Option<bool>(
    name: "--verbose",
    description: "Show verbose messages of the process.");
verboseOption.AddAlias("-v");

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

rootCommand.AddOption(workingFolderOption);
rootCommand.AddOption(configFileOption);
rootCommand.AddOption(outputFolderOption);
rootCommand.AddOption(verboseOption);

var initCommand = new Command("init", "Intialize a configuration file in the current directory if it doesn't exist yet.");
rootCommand.Add(initCommand);

// handle the execution of the root command
rootCommand.SetHandler(async (context) =>
{
    // setup logging
    SetLogLevel(context);

    LogParameters(
        context.ParseResult.GetValueForOption(configFileOption)!,
        context.ParseResult.GetValueForOption(outputFolderOption),
        context.ParseResult.GetValueForOption(workingFolderOption));

    // execute the generator
    context.ExitCode = (int)await AssembleDocumentationAsync(
        context.ParseResult.GetValueForOption(configFileOption)!,
        context.ParseResult.GetValueForOption(outputFolderOption),
        context.ParseResult.GetValueForOption(workingFolderOption));
});

// handle the execution of the root command
initCommand.SetHandler(async (context) =>
{
    // setup logging
    SetLogLevel(context);

    // execute the configuration file initializer
    context.ExitCode = (int)await GenerateConfigurationFile();
});

return await rootCommand.InvokeAsync(args);

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
    DirectoryInfo? workingFolder)
{
    // setup services
    ILogger logger = GetLogger();
    IFileService fileService = new FileService();

    try
    {
        ReturnCode ret = ReturnCode.Normal;

        string folder = workingFolder?.FullName ?? Directory.GetCurrentDirectory();
        InventoryAction inventory = new(folder, configFile.FullName, outputFolder?.FullName, fileService, logger);
        ret &= await inventory.RunAsync();

        AssembleAction assemble = new(configFile.FullName, outputFolder?.FullName, fileService, logger);
        ret &= await assemble.RunAsync();

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
    DirectoryInfo? workingFolder)
{
    ILogger logger = GetLogger();

    logger!.LogInformation($"Configuration : {configFile.FullName}");
    if (outputFolder != null)
    {
        logger!.LogInformation($"Output  folder: {outputFolder.FullName}");
        return;
    }

    if (workingFolder != null)
    {
        logger!.LogInformation($"Working folder: {workingFolder.FullName}");
        return;
    }
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
ILogger GetLogger() => GetLoggerFactory().CreateLogger(nameof(DocAssembler));
