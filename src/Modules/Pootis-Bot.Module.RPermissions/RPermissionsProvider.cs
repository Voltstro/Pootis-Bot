using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using Pootis_Bot.Commands.Permissions;
using Pootis_Bot.Config;
using Pootis_Bot.Module.RPermissions.Entities;

namespace Pootis_Bot.Module.RPermissions;

internal class RPermissionsProvider : IPermissionProvider
{
    private readonly RPermissionsConfig config;

    public RPermissionsProvider()
    {
        config = Config<RPermissionsConfig>.Instance;
    }

    public Task<PermissionResult> OnExecuteSlashCommand(SlashCommandInfo info, SocketInteractionContext context)
    {
        if (!config.DoesServerExist(context.Guild.Id))
            return Task.FromResult(PermissionResult.FromSuccess());

        RPermissionServer server = config.GetOrCreateServer(context.Guild.Id);
        RPerm? perm = server.SlashCommandGetPerm(info);
        if (perm == null) //No permission for that command, so all good
            return Task.FromResult(PermissionResult.FromSuccess());

        if (context.User is SocketGuildUser user)
            return Task.FromResult(perm.Roles.Any(role => user.Roles.Any(x => x.Id == role))
                ? PermissionResult.FromSuccess()
                : PermissionResult.FromError("You lack sufficient permissions to execute that command!"));

        return Task.FromResult(PermissionResult.FromSuccess());
    }
}