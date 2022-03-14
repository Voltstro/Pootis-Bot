using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Pootis_Bot.Core.Logging;
using Pootis_Bot.Core.Managers;
using Pootis_Bot.Entities;
using Pootis_Bot.Helpers;
using Pootis_Bot.Services;
using Pootis_Bot.Structs.Server;

namespace Pootis_Bot.Events
{
	/// <summary>
	/// Handles role client events
	/// </summary>
	public class RoleEvents
	{
		public async Task RoleDeleted(SocketRole role)
		{
			try
			{
				SocketGuild guild = role.Guild;
				ServerList server = ServerListsManager.GetServer(guild);

				//Setup the dm channel even though we might not even use it just makes it so I don't have to repeat this a whole bunch of times.
				IDMChannel dm = await guild.Owner.CreateDMChannelAsync();

				//The rule role was deleted
				if (role.Id == server.RuleRoleId)
				{
					server.RuleEnabled = false;
					ServerListsManager.SaveServerList();

					await dm.SendMessageAsync(
						$"Your rule reaction on the Discord server **{guild.Name}** has been disabled due to the role being deleted.\n" +
						"You can enable it again after setting a new role with the command `setuprulerole` and then enabling the feature again with `togglerulereaction`.");

					return;
				}

				//Check all server role points roles
				List<ServerRolePoints> rolePointsToRemove =
					server.ServerRolePoints.Where(rolePoint => role.Id == rolePoint.RoleId).ToList();
				foreach (ServerRolePoints toRemove in rolePointsToRemove)
				{
					await dm.SendMessageAsync(
						$"The **{role.Name}** was deleted which was apart of the {toRemove.PointsRequired} server points role. This server points role was deleted. ({guild.Name})");

					server.ServerRolePoints.Remove(toRemove);
					ServerListsManager.SaveServerList();
					return;
				}

				//Check to see if all the role to role pings still exist
				List<ServerRoleToRoleMention> rolesToRemove = server.RoleToRoleMentions
					.Where(roles => roles.RoleId == role.Id || roles.RoleNotToMentionId == role.Id).ToList();
				foreach (ServerRoleToRoleMention roleToRemove in rolesToRemove)
				{
					await dm.SendMessageAsync(
						$"The **{role.Name}** role was deleted which was apart of the **{roleToRemove.RoleNotToMentionId}** => **{roleToRemove.RoleId}**. This role to role ping was deleted. ({guild.Name})");

					server.RoleToRoleMentions.Remove(roleToRemove);
					ServerListsManager.SaveServerList();
					return;
				}

				//Check all permission roles
				BotCheckServerSettings.CheckServerPerms(server);
			}
			catch (Exception ex)
			{
				Logger.Error("An error occured while managing role deleted event! {@Exception}", ex);
			}
		}

		public async Task RoleUpdated(SocketRole before, SocketRole after)
		{
			try
			{
				SocketGuild guild = before.Guild;
				ServerList server = ServerListsManager.GetServer(guild);

				IDMChannel dm = await guild.Owner.CreateDMChannelAsync();

				//Check all server role pings to make sure they are still mentionable
				List<ServerRoleToRoleMention> rolesToRemove = server.RoleToRoleMentions
					.Where(roleToRole => roleToRole.RoleId == after.Id && !after.IsMentionable).ToList();
				foreach (ServerRoleToRoleMention roleToRemove in rolesToRemove)
				{
					await dm.SendMessageAsync(
						$"The **{after.Name}** role was changed to not mentionable so it was deleted from the **{RoleUtils.GetGuildRole(guild, roleToRemove.RoleNotToMentionId).Name}** => **{RoleUtils.GetGuildRole(guild, roleToRemove.RoleId).Name}** role to role ping list. ({guild.Name})");

					server.RoleToRoleMentions.Remove(roleToRemove);
					ServerListsManager.SaveServerList();

					return;
				}
			}
			catch (Exception ex)
			{
				Logger.Error("An error occured while managing role updated event! {@Exception}", ex);
			}
		}
	}
}