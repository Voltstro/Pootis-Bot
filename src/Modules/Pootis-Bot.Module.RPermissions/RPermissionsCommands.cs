using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Cysharp.Text;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Pootis_Bot.Config;
using Pootis_Bot.Module.RPermissions.Entities;

namespace Pootis_Bot.Module.RPermissions
{
    [Group("perm")]
    [Summary("Provides commands for setting up permissions")]
    public class RPermissionsCommands : ModuleBase<SocketCommandContext>
    {
        private readonly CommandService commandService;
        private readonly RPermissionsConfig config;

        public RPermissionsCommands(CommandService commandService)
        {
            this.commandService = commandService;
            config = Config<RPermissionsConfig>.Instance;
        }

        #region Add Permissions
        
        [Command("add")]
        [Summary("Adds a permission to a command")]
        public async Task AddPermission(SocketRole role, string command)
        {
            //Try to find the command first
            CommandSearchResult result = FindCommand(command);
            if (!result.IsSuccess)
            {
                await Context.Channel.SendMessageAsync(result.ErrorReason);
                return;
            }
            
            await AddPermissionInternal(result, role, command, 0);
        }
        
        [Command("add")]
        [Summary("Adds a permission to a command, the selection is used to choose which command to add if it has an override.")]
        public async Task AddPermission(SocketRole role, string command, int selection)
        {
            //Try to find the command first
            CommandSearchResult result = FindCommand(command);
            if (!result.IsSuccess && result.Error == CommandError.Unsuccessful)
            {
                await Context.Channel.SendMessageAsync(result.ErrorReason);
                return;
            }
            
            await AddPermissionInternal(result, role, command, selection);
        }
        
        /// <summary>
        ///     Adds a permission to a command
        /// </summary>
        /// <param name="result"></param>
        /// <param name="role"></param>
        /// <param name="command"></param>
        /// <param name="selection"></param>
        private async Task AddPermissionInternal(CommandSearchResult result, IRole role, string command, int selection)
        {
            //Check selection
            if (selection > result.SearchResult.Commands.Count)
            {
                await Context.Channel.SendMessageAsync("That selection is too high!");
                return;
            }

            //Now to add the command permission
            CommandMatch commandMatch = result.SearchResult.Commands[selection];

            RPermissionServer server = config.GetOrCreateServer(Context.Guild.Id);
            RPerm commandPermissions = server.GetPermissionForCommand(commandMatch.Command) ?? server.AddCommandPermission(commandMatch.Command);
            if (commandPermissions.Roles.Contains(role.Id))
            {
                await Context.Channel.SendMessageAsync("That role is already added as a permission to that command!");
                return;
            }
            
            commandPermissions.AddRole(role.Id);
            config.Save();

            await Context.Channel.SendMessageAsync(
                $"Added the permission {role.Name} as a requirement for the command `{command}`.");
        }

        #endregion

        #region Remove Permissions

        [Command("remove")]
        [Summary("Removes an entire command's permissions")]
        public async Task RemovePermission(string command)
        {
            //Find the command
            CommandSearchResult result = FindCommand(command);
            if (!result.IsSuccess)
            {
                await Context.Channel.SendMessageAsync(result.ErrorReason);
                return;
            }

            await RemovePermissionInternal(result, 0, null);
        }
        
        [Command("remove")]
        [Summary("Removes an entire command's permissions")]
        public async Task RemovePermission(string command, int selection)
        {
            //Find the command
            CommandSearchResult result = FindCommand(command);
            if (!result.IsSuccess && result.Error == CommandError.Unsuccessful)
            {
                await Context.Channel.SendMessageAsync(result.ErrorReason);
                return;
            }
            
            await RemovePermissionInternal(result, selection, null);
        }
        
        [Command("remove")]
        [Summary("Removes an entire command's permissions")]
        public async Task RemovePermission(SocketRole role, string command)
        {
            //Find the command
            CommandSearchResult result = FindCommand(command);
            if (!result.IsSuccess)
            {
                await Context.Channel.SendMessageAsync(result.ErrorReason);
                return;
            }
            
            await RemovePermissionInternal(result, 0, role);
        }
        
        [Command("remove")]
        [Summary("Removes an entire command's permissions")]
        public async Task RemovePermission(SocketRole role, string command, int selection)
        {
            //Find the command
            CommandSearchResult result = FindCommand(command);
            if (!result.IsSuccess && result.Error == CommandError.Unsuccessful)
            {
                await Context.Channel.SendMessageAsync(result.ErrorReason);
                return;
            }
            
            await RemovePermissionInternal(result, selection, role);
        }

        /// <summary>
        ///     Removes a permission from a command
        /// </summary>
        /// <param name="result"></param>
        /// <param name="selection"></param>
        /// <param name="role"></param>
        private async Task RemovePermissionInternal(CommandSearchResult result, int selection, [AllowNull] IRole role)
        {
            //Check selection
            if (selection > result.SearchResult.Commands.Count)
            {
                await Context.Channel.SendMessageAsync("That selection is too high!");
                return;
            }
            
            if (!config.DoesServerExist(Context.Guild.Id))
            {
                await Context.Channel.SendMessageAsync("There are no commands that have permissions in this server.");
                return;
            }
            
            CommandMatch commandMatch = result.SearchResult.Commands[selection];

            RPermissionServer server = config.GetOrCreateServer(Context.Guild.Id);
            RPerm perm = server.GetPermissionForCommand(commandMatch.Command);
            if (perm == null)
            {
                await Context.Channel.SendMessageAsync("That command has no permissions!");
                return;
            }

            //Remove everything
            if (role == null)
            {
                server.Permissions.Remove(perm);
                await Context.Channel.SendMessageAsync("All permissions were removed for that command.");
            }
            else
            {
                //The roles doesn't exist as apart of the permissions
                if (!perm.DoesRoleExist(role.Id))
                {
                    await Context.Channel.SendMessageAsync(
                        "That role is already not apart of the permissions for that command!");
                    return;
                }

                //Remove the role
                perm.Roles.Remove(role.Id);
                
                //If no roles are associated with that command, then remove the command entirely
                if (perm.Roles.Count == 0)
                    server.Permissions.Remove(perm);

                await Context.Channel.SendMessageAsync("The role was removed from the permissions.");
            }

            //If the server has no role, we might as well remove it.
            if (server.Permissions.Count == 0)
                config.Servers.Remove(server);

            config.Save();
        }

        #endregion
        
        #region Get Permissions

        [Command("get")]
        public async Task GetPermissions(string command)
        {
            //Find the command
            CommandSearchResult result = FindCommand(command);
            if (!result.IsSuccess)
            {
                await Context.Channel.SendMessageAsync(result.ErrorReason);
                return;
            }
            
            await GetPermissionsInternal(result, 0);
        }
        
        [Command("get")]
        public async Task GetPermissions(string command, int selection)
        {
            //Find the command
            CommandSearchResult result = FindCommand(command);
            if (!result.IsSuccess && result.Error == CommandError.Unsuccessful)
            {
                await Context.Channel.SendMessageAsync(result.ErrorReason);
                return;
            }
            
            await GetPermissionsInternal(result, selection);
        }
        
        /// <summary>
        ///     Gets a command's permissions
        /// </summary>
        private async Task GetPermissionsInternal(CommandSearchResult result, int selection)
        {
            //Check selection
            if (selection > result.SearchResult.Commands.Count)
            {
                await Context.Channel.SendMessageAsync("That selection is too high!");
                return;
            }

            //Get all the commands permissions (if it has any)
            CommandMatch commandMatch = result.SearchResult.Commands[selection];

            RPerm perm = config.GetOrCreateServer(Context.Guild.Id).GetPermissionForCommand(commandMatch.Command);
            if (perm == null)
            {
                await Context.Channel.SendMessageAsync("That command has no permissions associated with it.");
                return;
            }

            Utf16ValueStringBuilder sb = ZString.CreateStringBuilder();
            sb.Append($"`{commandMatch.Command.Name}` permissions:");
            
            foreach (ulong role in perm.Roles)
            {
                sb.Append($"\n{Context.Guild.GetRole(role).Name}");
            }
            
            await Context.Channel.SendMessageAsync(sb.ToString());
            sb.Dispose();
        }
        
        #endregion

        /// <summary>
        ///     Finds a command
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        private CommandSearchResult FindCommand(string input)
        {
            SearchResult search = commandService.Search(Context, input);
            if(!search.IsSuccess)
                return CommandSearchResult.FromError("No commands were found!", CommandError.Unsuccessful, search);

            if (search.Commands.Count != 1)
                return CommandSearchResult.FromError("Multiple commands were found! Use the selection to choose which one to select.", CommandError.MultipleMatches, search);
            
            return CommandSearchResult.FromSuccess(search);
        }
    }
}