using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Pootis_Bot.Core;
using Pootis_Bot.Core.Managers;
using Pootis_Bot.Entities;
using Pootis_Bot.Structs;

namespace Pootis_Bot.Services
{
	public class BotCheckServerSettings
	{
		/// <summary>
		/// Checks all server settings, auto vc channels, active vc channels and the welcome message
		/// </summary>
		/// <param name="client"></param>
		/// <returns></returns>
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

				//Active voice channels to delete
				List<ulong> toDeleteActiveChannels = new List<ulong>();
				foreach (ulong voiceChannel in server.ActiveAutoVoiceChannels)
				{
					//If the channel doesn't exist anymore remove it from the active voice channels
					if (client.GetGuild(server.GuildId).GetVoiceChannel(voiceChannel) == null)
					{
						toDeleteActiveChannels.Add(voiceChannel);
						somethingChanged = true;
						continue;
					}

					//Also remove and delete the channel if there is no one in the channel
					if (client.GetGuild(server.GuildId).GetVoiceChannel(voiceChannel).Users.Count != 0) continue;

					toDeleteActiveChannels.Add(voiceChannel);
					
					somethingChanged = true;
				}

				//To avoid System.InvalidOperationException remove all of the objects from the list after
				foreach (ulong toDeleteFromActiveVoiceChannelList in toDeleteActiveChannels)
				{
					server.ActiveAutoVoiceChannels.Remove(toDeleteFromActiveVoiceChannelList);

					if(client.GetGuild(server.GuildId).GetVoiceChannel(toDeleteFromActiveVoiceChannelList) != null)
#pragma warning disable 4014
						client.GetGuild(server.GuildId).GetVoiceChannel(toDeleteFromActiveVoiceChannelList).DeleteAsync();
#pragma warning restore 4014
				}

				List<VoiceChannel> autoVcChannelsToDelete = server.AutoVoiceChannels.Where(voiceChannel => client.GetGuild(server.GuildId).GetVoiceChannel(voiceChannel.Id) == null).ToList();

				foreach (VoiceChannel toDelete in autoVcChannelsToDelete)
				{
					server.AutoVoiceChannels.Remove(toDelete);
					somethingChanged = true;
				}
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
