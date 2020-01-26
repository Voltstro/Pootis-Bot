using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Pootis_Bot.Core.Managers;
using Pootis_Bot.Entities;
using Pootis_Bot.Helpers;

namespace Pootis_Bot.Modules.Server.Setup
{
	public class ServerSetupOptRoles : ModuleBase<SocketCommandContext>
	{
		// Module Information
		// Original Author  - Creepysin
		// Description      - Provides commands for setting up opt roles
		// Contributors     - Creepysin, 

		//TODO: We are gonna rename theses as 'Opt Roles'. So as in they are roles you opt into.
		[Command("setup add optrole")]
		[Summary("Adds a opt role")]
		[RequireBotPermission(GuildPermission.ManageRoles)]
		public async Task RoleGiveAdd(string optRoleBaseName, string roleToGiveName, [Remainder] string requiredRole = "")
		{
			SocketRole roleToAssign = RoleUtils.GetGuildRole(Context.Guild, roleToGiveName);

			//Check to make sure the role exists first
			if (roleToAssign == null)
			{
				await Context.Channel.SendMessageAsync($"No role under the name '{roleToGiveName}' exists!");
				return;
			}

			SocketRole socketRoleRequired = null;

			//If a required role was specified, check to make sure it exists
			if (!string.IsNullOrWhiteSpace(requiredRole))
			{
				socketRoleRequired = RoleUtils.GetGuildRole(Context.Guild, requiredRole);
				if (socketRoleRequired == null)
				{
					await Context.Channel.SendMessageAsync($"Role {requiredRole} doesn't exist!");
					return;
				}
			}

			ServerList server = ServerListsManager.GetServer(Context.Guild);

			//Check to make sure a role give doesn't already exist first
			if (server.GetRoleGive(optRoleBaseName) != null)
			{
				await Context.Channel.SendMessageAsync($"A opt role with the name '{optRoleBaseName}' already exist!");
				return;
			}

			RoleGive roleGive = new RoleGive
			{
				Name = optRoleBaseName,
				RoleToGiveId = roleToAssign.Id,
				RoleRequiredId = 0
			};

			if (socketRoleRequired != null)
				roleGive.RoleRequiredId = socketRoleRequired.Id;

			server.RoleGives.Add(roleGive);
			ServerListsManager.SaveServerList();

			await Context.Channel.SendMessageAsync($"The opt role was created with the name of **{optRoleBaseName}**.");
		}

		[Command("setup remove optrole")]
		[Summary("Removes a role give")]
		public async Task RoleGiveRemove(string optRoleName)
		{
			ServerList server = ServerListsManager.GetServer(Context.Guild);

			//Make sure the opt role exists first
			RoleGive optRole = server.GetRoleGive(optRoleName);
			if (optRole == null)
			{
				await Context.Channel.SendMessageAsync($"There is no opt role with the name '{optRoleName}''.");
				return;
			}

			//Remove it
			server.RoleGives.Remove(optRole);
			ServerListsManager.SaveServerList();

			await Context.Channel.SendMessageAsync($"Removed opt role '{optRoleName}'.");
		}
	}
}
