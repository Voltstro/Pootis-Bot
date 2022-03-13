using Discord.Commands;

namespace Pootis_Bot.Commands;

internal class CommandPermissionResult : IResult
{
    public CommandError? Error { get; private init; }
    public string ErrorReason { get; private init; }
    public bool IsSuccess { get; private init; }

    public static CommandPermissionResult FromSuccess()
    {
        return new CommandPermissionResult
        {
            IsSuccess = true
        };
    }

    public static CommandPermissionResult FromError(string errorReason)
    {
        return new CommandPermissionResult
        {
            Error = CommandError.UnmetPrecondition,
            ErrorReason = errorReason,
            IsSuccess = false
        };
    }
}