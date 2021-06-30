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
        
        [Command("add")]
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
            
            await GetPermissionsInternal(result, command, 0);
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
            
            await GetPermissionsInternal(result, command, selection);
        }
        
        #endregion

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

        /// <summary>
        ///     Gets a command's permissions
        /// </summary>
        private async Task GetPermissionsInternal(CommandSearchResult result, string command, int selection)
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