using Discord.Interactions;

namespace Pootis_Bot.Module.RPermissions.Entities;

internal class CommandSearchResult
{
    public string? ErrorReason { get; private init; }
    public bool IsSuccess { get; private init; }

    public SlashCommandInfo? SlashCommand { get; private init; }

    public static CommandSearchResult FromSuccess(SlashCommandInfo command)
    {
        return new()
        {
            SlashCommand = command,
            IsSuccess = true
        };
    }

    public static CommandSearchResult FromError(string errorReason)
    {
        return new()
        {
            IsSuccess = false,
            ErrorReason = errorReason
        };
    }
}