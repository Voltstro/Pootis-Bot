using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;
using Pootis_Bot.Core.Managers;
using Pootis_Bot.Entities;
using Pootis_Bot.Helpers;
using Pootis_Bot.Preconditions;
using Pootis_Bot.Structs.Server;

namespace Pootis_Bot.Modules.Server.Setup
{
	public class ServerSetupRolePings : ModuleBase<SocketCommandContext>
	{
		[Command("setup add roleping")]
		[Alias("setup add role ping")]
		[Summary("Adds a role to role ping")]
		[RequireGuildOwner]
		public async Task AddRoleToRoleMention(string roleToChangeName, string roleToNotAllowToMention)
		{
			SocketRole roleNotToMention = RoleUtils.GetGuildRole(Context.Guild, roleToChangeName);
			SocketRole role = RoleUtils.GetGuildRole(Context.Guild, roleToNotAllowToMention);

			if (roleNotToMention == null || role == null)
			{
				await Context.Channel.SendMessageAsync(
					$"Either the **{roleToChangeName}** role doesn't exist or the **{roleToNotAllowToMention}** role doesn't exist!");
				return;
			}

			if (!role.IsMentionable)
			{
				await Context.Channel.SendMessageAsync($"The **{role}** role is already not mentionable by anyone!");
				return;
			}

			ServerListsManager.GetServer(Context.Guild).CreateRoleToRoleMention(roleNotToMention.Id, role.Id);
			ServerListsManager.SaveServerList();

			await Context.Channel.SendMessageAsync(
				$"The **{roleNotToMention.Name}** role will not be allowed to mention the **{role.Name}** role.");
		}

		[Command("setup remove roleping")]
		[Alias("setup remove role ping")]
		[Summary("Removes a role to role ping")]
		[RequireGuildOwner]
		public async Task RemoveRoleToRoleMention(string roleToChangeName, string roleAllowedToMentionName)
		{
			SocketRole roleNotToMention = RoleUtils.GetGuildRole(Context.Guild, roleToChangeName);
			SocketRole role = RoleUtils.GetGuildRole(Context.Guild, roleAllowedToMentionName);

			if (roleNotToMention == null || role == null)
			{
				await Context.Channel.SendMessageAsync(
					$"Either the **{roleToChangeName}** role doesn't exist or the **{roleAllowedToMentionName}** role doesn't exist!");
				return;
			}

			ServerList server = ServerListsManager.GetServer(Context.Guild);
			List<ServerRoleToRoleMention> roleToRoleMentionsWithRole = server.GetRoleToRoleMention(role.Id);

			if (roleToRoleMentionsWithRole.Count == 0)
			{
				await Context.Channel.SendMessageAsync($"The **{role}** role doesn't have any preventions on it!");
				return;
			}

			foreach (ServerRoleToRoleMention roleMention in roleToRoleMentionsWithRole)
			{
				//We found it
				if (roleMention.RoleNotToMentionId == roleNotToMention.Id)
				{
					server.RoleToRoleMentions.Remove(roleMention);
					await Context.Channel.SendMessageAsync(
						$"The **{roleNotToMention.Name}** role can now mention the **{role.Name}** role.");

					ServerListsManager.SaveServerList();

					return;
				}

				await Context.Channel.SendMessageAsync(
					$"The **{roleNotToMention.Name}** role can already mention the **{role}** role.");
			}
		}

		[Command("rolepings")]
		[Alias("role pings")]
		[Summary("Gets all role to role pings")]
		[RequireGuildOwner]
		public async Task GetRolePings()
		{
			ServerList server = ServerListsManager.GetServer(Context.Guild);

			StringBuilder builder = new StringBuilder();
			builder.Append("__**Role to Roles**__\n```");

			foreach (ServerRoleToRoleMention roleToRole in server.RoleToRoleMentions)
				builder.Append(
					$"{RoleUtils.GetGuildRole(Context.Guild, roleToRole.RoleNotToMentionId).Name} =====> {RoleUtils.GetGuildRole(Context.Guild, roleToRole.RoleId).Name}\n");

			builder.Append("```");

			await Context.Channel.SendMessageAsync(builder.ToString());
		}
	}
}
