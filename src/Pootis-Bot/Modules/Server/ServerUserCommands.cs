using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Pootis_Bot.Core.Managers;
using Pootis_Bot.Entities;
using Pootis_Bot.Helpers;

namespace Pootis_Bot.Modules.Server
{
	public class ServerUserCommands : ModuleBase<SocketCommandContext>
	{
		// Module Information
		// Original Author  - Voltstro
		// Description      - Commands for users, but are more server-to-server related
		// Contributors     - Voltstro, 

		[Command("role")]
		[Alias("get optrole", "optrole", "getrole")]
		[Summary("Gives you an opt role, if it exists and if you meet the conditions (if any)")]
		[RequireBotPermission(GuildPermission.ManageRoles)]
		public async Task GiveOptRole([Remainder] string optRoleName = "")
		{
			SocketGuildUser user = (SocketGuildUser) Context.User;
			if (user == null) return;

			//Make sure the imputed optRoleName is not empty or null
			if (string.IsNullOrWhiteSpace(optRoleName))
			{
				await Context.Channel.SendMessageAsync("You need to input an opt role name!");
				return;
			}

			//Get the opt role
			OptRole optRole = ServerListsManager.GetServer(Context.Guild).GetOptRole(optRoleName);
			if (optRole == null) //And make sure it exists!
			{
				await Context.Channel.SendMessageAsync("That opt role doesn't exist!");
				return;
			}

			//If the opt role has a required role, make sure the user has it
			if (optRole.RoleRequiredId != 0)
				//The user doesn't have the required role
				if (!user.UserHaveRole(optRole.RoleRequiredId))
				{
					await Context.Channel.SendMessageAsync("You do not meet the requirements to get this opt role!");
					return;
				}

			//Give the user the role
			await user.AddRoleAsync(RoleUtils.GetGuildRole(Context.Guild, optRole.RoleToGiveId));

			//Say to the user that they now have the role
			await Context.Channel.SendMessageAsync(
				$"You now have the **{RoleUtils.GetGuildRole(Context.Guild, optRole.RoleToGiveId).Name}** role, {user.Mention}.");
		}

		[Command("optroles")]
		[Summary("Gets all opt roles that are on this server")]
		[RequireBotPermission(GuildPermission.ManageRoles)]
		public async Task GetOptRoles()
		{
			//Get all opt roles
			OptRole[] optRoles = ServerListsManager.GetServer(Context.Guild).RoleGives.ToArray();

			//Setup string builder
			StringBuilder message = new StringBuilder();
			message.Append("**Opt Roles**\n");

			if (optRoles.Length == 0)
				//There are no opt roles
				message.Append("There are no opt roles!");
			else
				foreach (OptRole optRole in optRoles)
				{
					//There are opt roles, so add them to the String Builder
					IRole role = RoleUtils.GetGuildRole(Context.Guild, optRole.RoleToGiveId);

					message.Append(optRole.RoleRequiredId == 0
						? $"`{optRole.Name}` -> **{role.Name}**\n"
						: $"`{optRole.Name}` -> **{role.Name}** (Requires **{RoleUtils.GetGuildRole(Context.Guild, optRole.RoleRequiredId).Name}**)\n");
				}

			await Context.Channel.SendMessageAsync(message.ToString());
		}
	}
}