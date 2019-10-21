using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Pootis_Bot.Core;
using Pootis_Bot.Entities;
using Pootis_Bot.Structs;

namespace Pootis_Bot.Services
{
	public class BotCheckServerSettings
	{
		public static async Task CheckConnectedServerSettings(DiscordSocketClient client)
		{
			Global.Log("Checking pre-connected server settings...");

			//To avoid saving possibly 100 times we will only save once if something has changed
			bool somethingChanged = false;

			List<ServerList> serversToRemove = new List<ServerList>();

			foreach (ServerList server in ServerListsManager.Servers)
			{
				//The bot is not longer in this guild, remove it from the server settings
				if (client.GetGuild(server.GuildId) == null)
				{
					somethingChanged = true;
					serversToRemove.Add(server);
					continue;
				}

				if (client.GetChannel(server.WelcomeChannelId) == null && server.WelcomeMessageEnabled)
				{
					somethingChanged = true;

					SocketGuild guild = client.GetGuild(server.GuildId);
					IDMChannel ownerDm = await guild.Owner.GetOrCreateDMChannelAsync();

					await ownerDm.SendMessageAsync(
						$"{guild.Owner.Mention}, your server **{guild.Name}** welcome channel has been disabled due to that it no longer exist since the last bot up time.\n" +
						$"You can enable it again with `{Global.BotPrefix}setupwelcomemessage` command and your existing message should stay.");

					server.WelcomeMessageEnabled = false;
					server.WelcomeChannelId = 0;
				}

				//Check to see if all the active channels don't have someone in it.
				List<ulong> deleteActiveChannels = new List<ulong>();

				foreach (ulong activeChannel in server.ActiveAutoVoiceChannels.Where(activeChannel =>
					client.GetChannel(activeChannel).Users.Count == 0))
				{
					await ((SocketVoiceChannel) client.GetChannel(activeChannel)).DeleteAsync();
					deleteActiveChannels.Add(activeChannel);
					somethingChanged = true;
				}

				//Check to see if all the auto voice channels are there
				List<VoiceChannel> deleteAutoChannels = new List<VoiceChannel>();
				foreach (VoiceChannel autoChannel in server.AutoVoiceChannels.Where(autoChannel =>
					client.GetChannel(autoChannel.Id) == null))
				{
					deleteAutoChannels.Add(autoChannel);
					somethingChanged = true;
				}

				//To avoid System.InvalidOperationException remove all of the objects from the list after
				foreach (ulong activeChannel in deleteActiveChannels)
					server.ActiveAutoVoiceChannels.Remove(activeChannel);

				foreach (VoiceChannel autoChannel in deleteAutoChannels) server.AutoVoiceChannels.Remove(autoChannel);
			}

			//Like all the other ones, we remove all the unnecessary servers after to avoid System.InvalidOperationException
			foreach (ServerList toRemove in serversToRemove)
			{
				Global.Log($"The bot is not longer in the {toRemove.GuildId}, Removing server settings...");
				ServerListsManager.Servers.Remove(toRemove);
			}

			//If a server was updated then save the ServerList.json file
			if (somethingChanged)
				ServerListsManager.SaveServerList();

			Global.Log("Checked all server settings.");
		}
	}
}
