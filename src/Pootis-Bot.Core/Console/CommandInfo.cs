using System.Diagnostics.CodeAnalysis;

namespace Pootis_Bot.Console;

internal struct CommandInfo
{
    internal string CommandSummary;

    [MaybeNull] internal ConsoleCommandManager.CommandArgumentsDelegate CommandArgumentDel;
    [MaybeNull] internal ConsoleCommandManager.CommandDelegate CommandDel;
}