using System.Threading.Tasks;
using Discord.Commands;
using Pootis_Bot.Commands.Permissions;

namespace Pootis_Bot.Module.RPermissions
{
    public class RPermissionsProvider : IPermissionProvider
    {
        public Task<PermissionResult> OnExecuteCommand(CommandInfo commandInfo, ICommandContext context)
        {
            return Task.FromResult(PermissionResult.FromSuccess());
        }
    }
}