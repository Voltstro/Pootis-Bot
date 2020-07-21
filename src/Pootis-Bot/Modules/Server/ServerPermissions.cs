using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;
using Pootis_Bot.Core.Managers;
using Pootis_Bot.Entities;
using Pootis_Bot.Helpers;
using Pootis_Bot.Preconditions;
using Pootis_Bot.Services;

namespace Pootis_Bot.Modules.Server
{
	public class ServerPermissions : ModuleBase<SocketCommandContext>
	{
		// Module Information
		// Original Author  - Voltstro
		// Description      - Anything permission related
		// Contributors     - Voltstro, 

		private readonly PermissionService perm;

		public ServerPermissions(CommandService commandService)
		{
			perm = new PermissionService(commandService);
		}

		[Command("perm")]
		[Summary("Adds or removes a command's permissions")]
		[RequireGuildOwner]
		public async Task Permission(string command, string subCmd, [Remainder] string[] roles)
		{
			switch (subCmd)
			{
				case "add":
					await perm.AddPerm(command, roles, Context.Channel, Context.Guild);
					break;
				default:
					await perm.RemovePerm(command, roles, Context.Channel, Context.Guild);
					break;
			}
		}

		[Command("perms")]
		[Summary("Gets a list of all commands that have permissions")]
		[Alias("permissions", "allperm", "allperms")]
		[RequireGuildOwner]
		public async Task Permissions()
		{
			StringBuilder sb = new StringBuilder();
			ServerList server = ServerListsManager.GetServer(Context.Guild);

			sb.Append("**__Permissions__**\n");

			foreach (ServerList.CommandPermission perm in server.CommandPermissions)
				sb.Append($"__`{perm.Command}`__\nRoles: {FormatRoles(perm.Roles, Context.Guild)}\n\n");

			await Context.Channel.SendMessageAsync(sb.ToString());
		}

		#region Functions

		private static string FormatRoles(IEnumerable<ulong> roles, SocketGuild guild)
		{
			return roles.Aggregate("", (current, role) => current + $"{RoleUtils.GetGuildRole(guild, role).Name}, ");
		}

		#endregion
	}
}