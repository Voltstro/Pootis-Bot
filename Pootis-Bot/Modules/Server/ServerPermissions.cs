using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;
using Pootis_Bot.Core;
using Pootis_Bot.Entities;
using Pootis_Bot.Preconditions;
using Pootis_Bot.Services;
using Pootis_Bot.Structs;

namespace Pootis_Bot.Modules.Server
{
	public class ServerPermissions : ModuleBase<SocketCommandContext>
	{
		// Module Information
		// Original Author   - Creepysin
		// Description      - Anything permission related
		// Contributors     - Creepysin, 

		private readonly PermissionService _perm;

		public ServerPermissions(CommandService commandService)
		{
			_perm = new PermissionService(commandService);
		}

		[Command("perm")]
		[RequireGuildOwner]
		public async Task Permission(string command, string subCmd, string role)
		{
			switch (subCmd)
			{
				case "add":
					await _perm.AddPerm(command, role, Context.Channel, Context.Guild);
					break;
				default:
					await _perm.RemovePerm(command, role, Context.Channel, Context.Guild);
					break;
			}
		}

		[Command("getbannedchannels")]
		[RequireGuildOwner]
		public async Task GetBannedChannels()
		{
			GlobalServerList server = ServerLists.GetServer(Context.Guild);
			StringBuilder final = new StringBuilder();
			final.Append("**All banned channels**: \n");

			foreach (ulong channel in server.BannedChannels) final.Append($"<#{channel}> (**Id**: {channel})\n");

			await Context.Channel.SendMessageAsync(final.ToString());
		}

		[Command("addbannedchannel")]
		[RequireGuildOwner]
		public async Task AddBannedChannel(SocketTextChannel channel)
		{
			ServerLists.GetServer(Context.Guild).GetOrCreateBannedChannel(channel.Id);
			ServerLists.SaveServerList();

			await Context.Channel.SendMessageAsync(
				$"Channel **{channel.Name}** has been added to the baned channels list for your server.");
		}

		[Command("removebannedchannel")]
		[RequireGuildOwner]
		public async Task RemoveBannedChannel(SocketTextChannel channel)
		{
			ServerLists.GetServer(Context.Guild).BannedChannels.Remove(channel.Id);
			ServerLists.SaveServerList();

			await Context.Channel.SendMessageAsync(
				$"Channel **{channel.Name}** was removed from your server's baned channel list.");
		}

		[Command("addroleping")]
		[RequireGuildOwner]
		public async Task AddRoleToRoleMention(string roleNotAllowedToMention, string role)
		{
			if ((Global.GetGuildRole(Context.Guild, roleNotAllowedToMention) == null) ||
			    (Global.GetGuildRole(Context.Guild, role) == null))
			{
				await Context.Channel.SendMessageAsync(
					$"Either the **{roleNotAllowedToMention}** role doesn't exist or the **{role}** role doesn't exist!");
				return;
			}

			if (!Global.GetGuildRole(Context.Guild, role).IsMentionable)
			{
				await Context.Channel.SendMessageAsync($"The **{role}** role is already not mentionable by anyone!");
				return;
			}

			ServerLists.GetServer(Context.Guild).CreateRoleToRoleMention(roleNotAllowedToMention, role);
			ServerLists.SaveServerList();

			await Context.Channel.SendMessageAsync(
				$"The **{roleNotAllowedToMention}** role will not be allowed to mention the **{role}** role.");
		}

		[Command("removeroleping")]
		[RequireGuildOwner]
		public async Task RemoveRoleToRoleMention(string roleNotAllowedToMention, string role)
		{
			if ((Global.GetGuildRole(Context.Guild, roleNotAllowedToMention) == null) ||
			    (Global.GetGuildRole(Context.Guild, role) == null))
			{
				await Context.Channel.SendMessageAsync(
					$"Either the **{roleNotAllowedToMention}** role doesn't exist or the **{role}** role doesn't exist!");
				return;
			}

			GlobalServerList server = ServerLists.GetServer(Context.Guild);
			List<RoleToRoleMention> roleToRoleMentionsWithRole = server.GetRoleToRoleMention(role);

			if (roleToRoleMentionsWithRole.Count == 0)
			{
				await Context.Channel.SendMessageAsync($"The **{role}** role doesn't have any preventions on it!");
				return;
			}

			foreach (RoleToRoleMention roleMention in roleToRoleMentionsWithRole)
			{
				//We found it
				if (roleMention.RoleNotToMention == roleNotAllowedToMention)
				{
					server.RoleToRoleMentions.Remove(roleMention);
					await Context.Channel.SendMessageAsync(
						$"The **{roleNotAllowedToMention}** role can now mention the **{role}** role.");

					ServerLists.SaveServerList();

					return;
				}

				await Context.Channel.SendMessageAsync(
					$"The **{roleNotAllowedToMention}** role can already mention the **{role}** role.");
			}
		}

		[Command("rolepings")]
		[RequireGuildOwner]
		public async Task GetRolePings()
		{
			GlobalServerList server = ServerLists.GetServer(Context.Guild);

			StringBuilder builder = new StringBuilder();
			builder.Append("__**Role to Roles**__\n");

			foreach (RoleToRoleMention roleToRole in server.RoleToRoleMentions)
				builder.Append($"{roleToRole.RoleNotToMention} x-> {roleToRole.Role}\n");

			await Context.Channel.SendMessageAsync(builder.ToString());
		}
	}
}