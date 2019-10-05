using System.Threading.Tasks;
using Discord.WebSocket;
using Pootis_Bot.Core;
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
			GlobalServerList serverList = ServerLists.GetServer(((SocketGuildChannel) channel).Guild);
			VoiceChannel voiceChannel = serverList.GetVoiceChannel(channel.Id);

			//If the channel deleted was an auto voice channel, remove it from the list.
			if (voiceChannel.Name == null) return Task.CompletedTask;

			serverList.VoiceChannels.Remove(voiceChannel);
			ServerLists.SaveServerList();

			return Task.CompletedTask;
		}
	}
}
