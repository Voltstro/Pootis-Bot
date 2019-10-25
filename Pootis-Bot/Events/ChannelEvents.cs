using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord.WebSocket;
using Pootis_Bot.Core.Managers;
using Pootis_Bot.Entities;
using Pootis_Bot.Structs;

namespace Pootis_Bot.Events
{
	/// <summary>
	/// Handles channel client events
	/// </summary>
	public class ChannelEvents
	{
		public Task ChannelDestroyed(SocketChannel channel)
		{
			ServerList serverList = ServerListsManager.GetServer(((SocketGuildChannel) channel).Guild);
			VoiceChannel voiceChannel = serverList.GetAutoVoiceChannel(channel.Id);

			List<ulong> activeVcsToRemove = serverList.ActiveAutoVoiceChannels.Where(activeVcs => activeVcs == channel.Id).ToList();

			//Removes active voice channel if deleted
			foreach (ulong toRemove in activeVcsToRemove)
			{
				serverList.ActiveAutoVoiceChannels.Remove(toRemove);
				ServerListsManager.SaveServerList();
				return Task.CompletedTask;
			}

			//If the channel deleted was an auto voice channel, remove it from the list.
			if (voiceChannel.Name == null) return Task.CompletedTask;

			serverList.AutoVoiceChannels.Remove(voiceChannel);
			ServerListsManager.SaveServerList();

			return Task.CompletedTask;
		}
	}
}
