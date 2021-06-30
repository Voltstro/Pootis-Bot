using Discord.Commands;

namespace Pootis_Bot.Module.RPermissions.Entities
{
    internal class CommandSearchResult : IResult
    {
        public CommandError? Error { get; init; }
        public string ErrorReason { get; init; }
        public bool IsSuccess { get; init; }
        
        public SearchResult SearchResult { get; init; }

        public static CommandSearchResult FromSuccess(SearchResult searchResult) => new CommandSearchResult
        {
            IsSuccess = true,
            Error = null,
            SearchResult = searchResult
        };
        
        public static CommandSearchResult FromError(string errorReason, CommandError error, SearchResult searchResult) => new CommandSearchResult
        {
            IsSuccess = false,
            ErrorReason = errorReason,
            Error = error,
            SearchResult = searchResult
        };
    }
}