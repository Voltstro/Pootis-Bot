using Discord.Commands;
using Discord.Interactions;
using IResult = Discord.Commands.IResult;

namespace Pootis_Bot.Module.RPermissions.Entities
{
    internal class CommandSearchResult : IResult
    {
        public CommandError? Error { get; init; }
        public string ErrorReason { get; init; }
        public bool IsSuccess { get; init; }
        
        public SlashCommandInfo SlashCommand { get; init; }

        public static CommandSearchResult FromSuccess(SlashCommandInfo command) => new CommandSearchResult
        {
            IsSuccess = true,
            Error = null,
            SlashCommand = command
        };
        
        public static CommandSearchResult FromError(string errorReason, CommandError error, SlashCommandInfo command) => new CommandSearchResult
        {
            IsSuccess = false,
            ErrorReason = errorReason,
            Error = error,
            SlashCommand = command
        };
    }
}