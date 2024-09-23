using System;
using Discord;
using Microsoft.Extensions.Logging;

namespace Pootis_Bot.Core;

public static class Utils
{
    public static LogLevel DiscordLogSeverityToLogLevel(LogSeverity logSeverity)
    {
        return logSeverity switch
        {
            LogSeverity.Critical => LogLevel.Critical,
            LogSeverity.Error => LogLevel.Error,
            LogSeverity.Warning => LogLevel.Warning,
            LogSeverity.Info => LogLevel.Information,
            LogSeverity.Verbose or LogSeverity.Debug => LogLevel.Debug,
            _ => throw new ArgumentOutOfRangeException(nameof(logSeverity), logSeverity, null)
        };
    }

    public static string Truncate(string value, int maxLength, string truncationSuffix = "...")
    {
        return value.Length > maxLength
            ? value[..(maxLength - truncationSuffix.Length)] + truncationSuffix
            : value;
    }
}