using System.Threading.Tasks;
using Discord.Rest;
using Discord.WebSocket;
using Pootis_Bot.Core.Managers;
using Pootis_Bot.Structs.Server;

namespace Pootis_Bot.Services.Audio
{
	/// <summary>
	/// Creates Auto Voice Channels
	/// </summary>
	public static class AutoVCChannelCreator
	{
		public static async Task<RestVoiceChannel> CreateAutoVCChannel(SocketGuild guild, string baseName)
		{
			RestVoiceChannel vcChannel =
				await (guild).CreateVoiceChannelAsync($"➕ New {baseName} VC");

			ServerVoiceChannel voiceChannel = new ServerVoiceChannel(vcChannel.Id, baseName);

			ServerListsManager.GetServer(guild).AutoVoiceChannels.Add(voiceChannel);
			ServerListsManager.SaveServerList();

			return vcChannel;
		}
	}
}