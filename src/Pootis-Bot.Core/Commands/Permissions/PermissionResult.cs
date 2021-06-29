using Discord.Commands;

namespace Pootis_Bot.Commands.Permissions
{
    public class PermissionResult : IResult
    {
        /// <inheritdoc />
        public CommandError? Error { get; private init; }
        
        /// <inheritdoc />
        public string ErrorReason { get; private init; }
        
        /// <inheritdoc />
        public bool IsSuccess { get; private init; }

        public static PermissionResult FromSuccess() => new PermissionResult
        {
            IsSuccess = true
        };

        public static PermissionResult FromError(string errorReason) => new PermissionResult
        {
            ErrorReason = errorReason,
            Error = CommandError.UnmetPrecondition,
            IsSuccess = false
        };
    }
}