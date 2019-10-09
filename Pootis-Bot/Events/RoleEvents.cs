using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Pootis_Bot.Core;
using Pootis_Bot.Entities;
using Pootis_Bot.Structs;

namespace Pootis_Bot.Events
{
	/// <summary>
	/// Handles role client events
	/// </summary>
	public class RoleEvents
	{
		public async Task RoleDeleted(SocketRole role)
		{
			SocketGuild guild = role.Guild;
			ServerList server = ServerLists.GetServer(guild);

			//Setup the dm channel even though we might not even use it just makes it so I don't have to repeat this a whole bunch of times.
			IDMChannel dm = await guild.Owner.GetOrCreateDMChannelAsync();

			//The rule role was deleted
			if (role.Id == server.RuleRoleId)
			{
				server.RuleEnabled = false;
				ServerLists.SaveServerList();

				
				await dm.SendMessageAsync($"Your rule reaction on the Discord server **{guild.Name}** has been disabled due to the role being deleted.\n" +
				                          $"You can enable it again after setting a new role with the command `setuprulerole` and then enabling the feature again with `togglerulereaction`.");

				return;
			}

			//Check to see if all the role to role pings still exist

			List<RoleToRoleMention> rolesToRemove = new List<RoleToRoleMention>();

			foreach (RoleToRoleMention roles in server.RoleToRoleMentions.Where(roles => roles.RoleId == role.Id || roles.RoleNotToMentionId == role.Id))
			{
				await dm.SendMessageAsync(
					$"The **{role.Name}** role was deleted which was apart of the **{roles.RoleNotToMentionId}** => **{roles.RoleId}**. This role to role ping was deleted. ({guild.Name})");

				rolesToRemove.Add(roles);
			}

			foreach (RoleToRoleMention roleToRemove in rolesToRemove)
			{
				server.RoleToRoleMentions.Remove(roleToRemove);
				ServerLists.SaveServerList();
			}
		}

		public async Task RoleUpdated(SocketRole before, SocketRole after)
		{
			SocketGuild guild = before.Guild;
			ServerList server = ServerLists.GetServer(guild);

			List<RoleToRoleMention> rolesToRemove = new List<RoleToRoleMention>();

			foreach (RoleToRoleMention roleToRole in server.RoleToRoleMentions.Where(roleToRole => roleToRole.RoleId == after.Id && !after.IsMentionable))
			{
				IDMChannel dm = await guild.Owner.GetOrCreateDMChannelAsync();

				await dm.SendMessageAsync(
					$"The **{after.Name}** role was changed to not mentionable so it was deleted from the **{Global.GetGuildRole(guild, roleToRole.RoleNotToMentionId).Name}** => **{Global.GetGuildRole(guild, roleToRole.RoleId).Name}** role to role ping list. ({guild.Name})");

				rolesToRemove.Add(roleToRole);
			}

			foreach (RoleToRoleMention roleToRemove in rolesToRemove)
			{
				server.RoleToRoleMentions.Remove(roleToRemove);
				ServerLists.SaveServerList();
			}
		}
	}
}
