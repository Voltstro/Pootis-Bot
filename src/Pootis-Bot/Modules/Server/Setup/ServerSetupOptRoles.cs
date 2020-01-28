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

		[Command("setup add optrole")]
		[Summary("Adds a opt role")]
		[RequireBotPermission(GuildPermission.ManageRoles)]
		public async Task AddOptRole(string optRoleBaseName, string roleToAssignName, [Remainder] string requiredRoleName = "")
		{
			SocketRole roleToAssign = RoleUtils.GetGuildRole(Context.Guild, roleToAssignName);

			//Check to make sure the role exists first
			if (roleToAssign == null)
			{
				await Context.Channel.SendMessageAsync("You need to input a valid role to give!");
				return;
			}

			//If a required role was specified, check to make sure it exists
			SocketRole requiredRole = null;
			if (!string.IsNullOrWhiteSpace(requiredRoleName))
			{
				requiredRole = RoleUtils.GetGuildRole(Context.Guild, requiredRoleName);
				if (requiredRole == null)
				{
					await Context.Channel.SendMessageAsync($"Role {requiredRoleName} doesn't exist!");
					return;
				}
			}

			ServerList server = ServerListsManager.GetServer(Context.Guild);

			//Check to make sure a role give doesn't already exist first
			if (server.GetOptRole(optRoleBaseName) != null)
			{
				await Context.Channel.SendMessageAsync($"An opt role with the name '{optRoleBaseName}' already exists!");
				return;
			}

			//Create and add our new opt role
			OptRole roleGive = new OptRole
			{
				Name = optRoleBaseName,
				RoleToGiveId = roleToAssign.Id,
				RoleRequiredId = 0
			};

			if (requiredRole != null)
				roleGive.RoleRequiredId = requiredRole.Id;

			server.RoleGives.Add(roleGive);
			ServerListsManager.SaveServerList();

			await Context.Channel.SendMessageAsync($"An opt role with the name `{optRoleBaseName}` was created.");
		}

		[Command("setup remove optrole")]
		[Summary("Removes a role give")]
		public async Task RoleGiveRemove(string optRoleName)
		{
			ServerList server = ServerListsManager.GetServer(Context.Guild);

			//Make sure the opt role exists first
			OptRole optRole = server.GetOptRole(optRoleName);
			if (optRole == null)
			{
				await Context.Channel.SendMessageAsync($"There is no opt role with the name '{optRoleName}'.");
				return;
			}

			//Remove it
			server.RoleGives.Remove(optRole);
			ServerListsManager.SaveServerList();

			await Context.Channel.SendMessageAsync($"An opt role with the name `{optRoleName}` was removed.");
		}
	}
}
