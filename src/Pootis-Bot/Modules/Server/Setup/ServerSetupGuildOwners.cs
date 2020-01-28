using System.Text;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;
using Pootis_Bot.Core.Managers;
using Pootis_Bot.Entities;
using Pootis_Bot.Preconditions;

namespace Pootis_Bot.Modules.Server.Setup
{
	public class ServerSetupGuildOwners : ModuleBase<SocketCommandContext>
	{
		[Command("setup add guildowner")]
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

		[Command("setup remove guildowner")]
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

		[Command("guildowners")]
		[Alias("guild owners", "listguildowners", "list guild owners")]
		[Summary("Lists all the owners of the server")]
		[RequireGuildOwner]
		public async Task GuildOwners()
		{
			ServerList server = ServerListsManager.GetServer(Context.Guild);

			StringBuilder builder = new StringBuilder();
			builder.Append("__**Guild Owners**__\n```");
			builder.Append($"{Context.Guild.Owner} (Server Owner)\n");

			foreach (ulong id in server.GuildOwnerIds)
				builder.Append(
					$"{Context.Guild.GetUser(id)}\n");

			builder.Append("```");

			await Context.Channel.SendMessageAsync(builder.ToString());
		}
	}
}
