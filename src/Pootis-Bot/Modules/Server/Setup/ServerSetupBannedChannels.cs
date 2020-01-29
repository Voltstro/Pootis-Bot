using System.Text;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;
using Pootis_Bot.Core.Managers;
using Pootis_Bot.Entities;
using Pootis_Bot.Preconditions;

namespace Pootis_Bot.Modules.Server.Setup
{
	public class ServerSetupBannedChannels : ModuleBase<SocketCommandContext>
	{
		[Command("setup bannedchannels")]
		[Alias("setup banned channels")]
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

		[Command("setup add bannedchannel")]
		[Alias("setup add banned channel")]
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

		[Command("setup remove bannedchannel")]
		[Alias("setup removed banned channel")]
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
	}
}
