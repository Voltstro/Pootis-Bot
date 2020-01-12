using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Pootis_Bot.Core.Managers;
using Pootis_Bot.Entities;
using Pootis_Bot.Helpers;

namespace Pootis_Bot.Modules.Server
{
	public class ServerSetup : ModuleBase<SocketCommandContext>
	{
		// Module Information
		// Original Author  - Creepysin
		// Description      - Helps the server owner set up the bot for use
		// Contributors     - Creepysin, 

		//TODO: We are gonna rename theses as 'Opt Roles'. So as in they are roles you opt into.
		[Command("rolegiveadd")]
		[Alias("role give add", "add role give")]
		[Summary("Assigns you a specified role if the user meets a requirement")]
		[RequireBotPermission(GuildPermission.ManageRoles)]
		public async Task RoleGiveAdd(string roleGiveName, string roleToGive, [Remainder] string roleRequired = "")
		{
			SocketRole roleToAssign = RoleUtils.GetGuildRole(Context.Guild, roleToGive);

			//Check to make sure the role exists first
			if (roleToAssign == null)
			{
				await Context.Channel.SendMessageAsync($"No role under the name '{roleToGive}' exists!");
				return;
			}

			SocketRole socketRoleRequired = null;

			//If a required role was specified, check to make sure it exists
			if (!string.IsNullOrWhiteSpace(roleRequired))
			{
				socketRoleRequired = RoleUtils.GetGuildRole(Context.Guild, roleRequired);
				if (socketRoleRequired == null)
				{
					await Context.Channel.SendMessageAsync($"Role {roleRequired} doesn't exist!");
					return;
				}
			}

			ServerList server = ServerListsManager.GetServer(Context.Guild);

			//Check to make sure a role give doesn't already exist first
			if (server.GetRoleGive(roleGiveName) != null)
			{
				await Context.Channel.SendMessageAsync($"A role give with the name '{roleGiveName}' already exist!");
				return;
			}

			RoleGive roleGive = new RoleGive
			{
				Name = roleGiveName,
				RoleToGiveId = roleToAssign.Id,
				RoleRequiredId = 0
			};

			if (socketRoleRequired != null)
				roleGive.RoleRequiredId = socketRoleRequired.Id;

			server.RoleGives.Add(roleGive);
			ServerListsManager.SaveServerList();

			await Context.Channel.SendMessageAsync($"The role give was created with the name of **{roleGiveName}**.");
		}

		[Command("rolegiveremove")]
		[Alias("role give remove", "remove role give")]
		[Summary("Removes a role give")]
		public async Task RoleGiveRemove(string roleGiveName)
		{
			ServerList server = ServerListsManager.GetServer(Context.Guild);
			RoleGive roleGive = server.GetRoleGive(roleGiveName);
			if (roleGive == null)
			{
				await Context.Channel.SendMessageAsync($"There is no role give with the name '{roleGiveName}''.");
				return;
			}

			server.RoleGives.Remove(roleGive);
			ServerListsManager.SaveServerList();

			await Context.Channel.SendMessageAsync($"Removed role give '{roleGiveName}'.'");
		}

		//TODO: Add the ability to change the server points amount
	}
}