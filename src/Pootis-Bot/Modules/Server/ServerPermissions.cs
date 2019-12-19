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
using Pootis_Bot.Structs.Server;

namespace Pootis_Bot.Modules.Server
{
	public class ServerPermissions : ModuleBase<SocketCommandContext>
	{
		// Module Information
		// Original Author  - Creepysin
		// Description      - Anything permission related
		// Contributors     - Creepysin, 

		private readonly PermissionService _perm;

		public ServerPermissions(CommandService commandService)
		{
			_perm = new PermissionService(commandService);
		}

		[Command("perm")]
		[Summary("Adds or removes command permission")]
		[RequireGuildOwner]
		public async Task Permission(string command, string subCmd, [Remainder] string[] roles)
		{
			switch (subCmd)
			{
				case "add":
					await _perm.AddPerm(command, roles, Context.Channel, Context.Guild);
					break;
				default:
					await _perm.RemovePerm(command, roles, Context.Channel, Context.Guild);
					break;
			}
		}

		[Command("perms")]
		[Alias("permissions", "allperm", "allperms")]
		[RequireGuildOwner]
		public async Task Permissions()
		{
			StringBuilder sb = new StringBuilder();
			ServerList server = ServerListsManager.GetServer(Context.Guild);

			sb.Append("**__Permissions__**\n");

			foreach (ServerList.CommandInfo perm in server.CommandInfos)
				sb.Append($"__`{perm.Command}`__\nRoles: {FormatRoles(perm.Roles, Context.Guild)}\n\n");

			await Context.Channel.SendMessageAsync(sb.ToString());
		}

		[Command("getbannedchannels")]
		[Alias("get banned channels")]
		[Summary("Gets all banned channels")]
		[RequireGuildOwner]
		public async Task GetBannedChannels()
		{
			ServerList server = ServerListsManager.GetServer(Context.Guild);
			StringBuilder final = new StringBuilder();
			final.Append("**All banned channels**: \n");

			foreach (ulong channel in server.BannedChannels) final.Append($"<#{channel}> (**Id**: {channel})\n");

			await Context.Channel.SendMessageAsync(final.ToString());
		}

		[Command("addbannedchannel")]
		[Alias("add banned channel")]
		[Summary("Adds a banned channel")]
		[RequireGuildOwner]
		public async Task AddBannedChannel(SocketTextChannel channel)
		{
			ServerList server = ServerListsManager.GetServer(Context.Guild);
			if (server.GetBannedChannel(channel.Id) == 0)
			{
				server.CreateBannedChannel(channel.Id);
				ServerListsManager.SaveServerList();

				await Context.Channel.SendMessageAsync(
					$"Channel **{channel.Name}** has been added to the banned channels list for your server.");
			}
			else
			{
				await Context.Channel.SendMessageAsync(
					$" Channel **{channel.Name}** is already apart of the banned channel list!");
			}
		}

		[Command("removebannedchannel")]
		[Alias("remove banned channel")]
		[Summary("Removes a banned channel")]
		[RequireGuildOwner]
		public async Task RemoveBannedChannel(SocketTextChannel channel)
		{
			ServerList server = ServerListsManager.GetServer(Context.Guild);
			if (server.GetBannedChannel(channel.Id) != 0)
			{
				server.BannedChannels.Remove(channel.Id);
				ServerListsManager.SaveServerList();

				await Context.Channel.SendMessageAsync(
					$"Channel **{channel.Name}** was removed from the banned channel list.");
			}
			else
			{
				await Context.Channel.SendMessageAsync(
					$"Channel **{channel.Name}** isn't apart of the banned channel list!");
			}
		}

		[Command("addroleping")]
		[Alias("add role ping")]
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

		[Command("removeroleping")]
		[Alias("remove role ping")]
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

		[Command("addguildowner")]
		[Alias("addowner", "add guild owner", "add owner")]
		[Summary("Adds a guild owner")]
		[RequireGuildOwner(false)]
		public async Task AddGuildOwner([Remainder] SocketGuildUser user)
		{
			if (user.Id == Context.Guild.OwnerId)
			{
				await Context.Channel.SendMessageAsync(
					"You cannot add the guild's owner! This person will already have access to all owner commands!");
				return;
			}

			if (user.IsBot)
			{
				await Context.Channel.SendMessageAsync("You cannot add a bot as an owner!");
				return;
			}

			ServerList server = ServerListsManager.GetServer(Context.Guild);
			if (server.GetAGuildOwner(user.Id) != 0)
			{
				await Context.Channel.SendMessageAsync($"**{user.Username}** is already a owner!");
				return;
			}

			if (!user.GuildPermissions.Administrator)
			{
				await Context.Channel.SendMessageAsync($"**{user.Username}** is not an administrator!");
				return;
			}

			server.GuildOwnerIds.Add(user.Id);
			ServerListsManager.SaveServerList();

			await Context.Channel.SendMessageAsync($"The user **{user.Username}** was added as a guild owner.");
		}

		[Command("removeguildowner")]
		[Alias("removeowner", "remove guild owner", "remove owner")]
		[Summary("Removes a guild owner")]
		[RequireGuildOwner(false)]
		public async Task RemoveGuildOwner([Remainder] SocketGuildUser user)
		{
			if (user.Id == Context.Guild.OwnerId)
			{
				await Context.Channel.SendMessageAsync(
					"You cannot remove the guild's owner! This person will already have access to all owner commands!");
				return;
			}

			ServerList server = ServerListsManager.GetServer(Context.Guild);
			if (server.GetAGuildOwner(user.Id) == 0)
			{
				await Context.Channel.SendMessageAsync($"**{user.Username}** isn't an owner!");
				return;
			}

			server.GuildOwnerIds.Remove(user.Id);
			ServerListsManager.SaveServerList();

			await Context.Channel.SendMessageAsync($"The user **{user.Username}** is no longer an owner.");
		}

		#region Functions

		private static string FormatRoles(IEnumerable<ulong> roles, SocketGuild guild)
		{
			return roles.Aggregate("", (current, role) => current + $"{RoleUtils.GetGuildRole(guild, role).Name}, ");
		}

		#endregion
	}
}