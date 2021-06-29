using Discord.Commands;

namespace Pootis_Bot.Commands
{
    internal class CommandSearchResult : IResult
    {
        public CommandError? Error { get; private init; }
        public string ErrorReason { get; private init; }
        public bool IsSuccess { get; private init; }
        
        public CommandMatch CommandMatch { get; private init; }
        public ParseResult ParseResult { get; private init; }

        public static CommandSearchResult FromSuccess(CommandMatch command, ParseResult parseResult)
        {
            return new CommandSearchResult
            {
                CommandMatch = command,
                ParseResult = parseResult,
                IsSuccess = true,
                Error = null
            };
        }

        public static CommandSearchResult FromError(CommandError error, string errorReason)
        {
            return new CommandSearchResult
            {
                Error = error,
                ErrorReason = errorReason,
                IsSuccess = false
            };
        }
    }
}