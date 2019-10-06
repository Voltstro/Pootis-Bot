using System.Threading.Tasks;
using Discord.Rest;
using Discord.WebSocket;
using Pootis_Bot.Entities;

namespace Pootis_Bot.Structs
{
	public class AutoVcChannelCreate
	{
		/// <summary>
		/// Sets up an auto vc channel with permissions from the base channel
		/// </summary>
		/// <param name="createdChannel">The newly created vc channel</param>
		/// <param name="baseChannel">The 'parent' channel</param>
		/// <param name="channel">The auto-vc</param>
		/// <param name="server"></param>
		/// <returns></returns>
		public static async Task SetupChannel(RestVoiceChannel createdChannel, SocketVoiceChannel baseChannel, VoiceChannel channel, GlobalServerList server)
		{
			int count = server.ActiveAutoVoiceChannels.Count + 1;
			await createdChannel.ModifyAsync(x =>
			{
				x.Bitrate = baseChannel.Bitrate;
				x.Name = channel.Name + " #" + count;
				x.Position = baseChannel.Position + 1;
				x.CategoryId = baseChannel.CategoryId;
			});
		}
	}
}
