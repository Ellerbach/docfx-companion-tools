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
var configFile = new Option<FileInfo>(
    name: "--config",
    description: "The configuration file for the assembled documentation.")
{
    IsRequired = true,
};
configFile.AddAlias("-c");

var outputFolder = new Option<DirectoryInfo>(
    name: "--outfolder",
    description: "Override the output folder for the assembled documentation in the config file.");
outputFolder.AddAlias("-o");

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

rootCommand.AddOption(configFile);
rootCommand.AddOption(outputFolder);
rootCommand.AddOption(verboseOption);

var initCommand = new Command("init", "Intialize a configuration file in the current directory if it doesn't exist yet.");
rootCommand.Add(initCommand);

// handle the execution of the root command
rootCommand.SetHandler(async (context) =>
{
    // setup logging
    SetLogLevel(context);

    LogParameters(
        context.ParseResult.GetValueForOption(configFile)!,
        context.ParseResult.GetValueForOption(outputFolder));

    // execute the generator
    context.ExitCode = (int)await AssembleDocumentationAsync(
        context.ParseResult.GetValueForOption(configFile)!,
        context.ParseResult.GetValueForOption(outputFolder));
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
        ConfigInitAction action = new(string.Empty, fileService, logger);
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
    DirectoryInfo? outputFolder)
{
    // setup services
    ILogger logger = GetLogger();
    IFileService fileService = new FileService();

    try
    {
        // WIP: should be inventory followed by assemble.
        AssembleAction assemble = new(configFile.FullName, outputFolder?.FullName, fileService, logger);
        ReturnCode ret = await assemble.RunAsync();

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
    DirectoryInfo? outputFolder)
{
    ILogger logger = GetLogger();

    logger!.LogInformation($"Configuration: {configFile.FullName}");
    if (outputFolder != null)
    {
        logger!.LogInformation($"Output folder: {outputFolder.FullName}");
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
