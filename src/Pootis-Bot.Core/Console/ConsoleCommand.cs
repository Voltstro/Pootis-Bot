using System;
using System.Diagnostics.CodeAnalysis;

namespace Pootis_Bot.Console;

/// <summary>
///     A command that can be executed in the console
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class ConsoleCommand : Attribute
{
    internal readonly string Command;

    internal readonly string CommandSummary;

    /// <summary>
    ///     Creates a new <see cref="ConsoleCommand" />
    /// </summary>
    /// <param name="command">What command to enter into the console</param>
    /// <param name="summary">A basic summary of the command</param>
    /// <exception cref="ArgumentNullException"></exception>
    public ConsoleCommand([DisallowNull] string command, [DisallowNull] string summary)
    {
        if (string.IsNullOrWhiteSpace(command))
            throw new ArgumentNullException(nameof(command));

        if (string.IsNullOrWhiteSpace(summary))
            throw new ArgumentNullException(nameof(summary));

        Command = command;
        CommandSummary = summary;
    }
}