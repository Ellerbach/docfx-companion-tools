// <copyright file="LogUtil.cs" company="DocFx Companion Tools">
// Copyright (c) DocFx Companion Tools. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
using System.Globalization;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;

namespace DocFxTocGenerator.Utils;

/// <summary>
/// Log utils.
/// </summary>
internal static class LogUtil
{
    /// <summary>
    /// Get the logger factory.
    /// </summary>
    /// <param name="logLevel1">Log level.</param>
    /// <returns>Logger factory.</returns>
    /// <exception cref="ArgumentOutOfRangeException">When an unknown log level is given.</exception>
    public static ILoggerFactory GetLoggerFactory(LogLevel logLevel1)
    {
        var serilogLevel = logLevel1 switch
        {
            LogLevel.Critical => LogEventLevel.Fatal,
            LogLevel.Error => LogEventLevel.Error,
            LogLevel.Warning => LogEventLevel.Warning,
            LogLevel.Information => LogEventLevel.Information,
            LogLevel.Debug => LogEventLevel.Debug,
            LogLevel.Trace => LogEventLevel.Verbose,
            _ => throw new ArgumentOutOfRangeException(nameof(logLevel1)),
        };

        var serilog = new LoggerConfiguration()
            .MinimumLevel.Is(serilogLevel)
            .WriteTo.Console(standardErrorFromLevel: LogEventLevel.Warning, outputTemplate: "{Message:lj}{NewLine}", formatProvider: CultureInfo.InvariantCulture)
            .CreateLogger();
        return LoggerFactory.Create(p => p.AddSerilog(serilog));
    }
}
