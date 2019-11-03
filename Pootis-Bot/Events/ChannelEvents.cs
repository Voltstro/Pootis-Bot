using System.Threading.Tasks;
using Discord.WebSocket;
using Pootis_Bot.Core.Managers;
using Pootis_Bot.Entities;
using Pootis_Bot.Services;

namespace Pootis_Bot.Events
{
	/// <summary>
	/// Handles channel client events
	/// </summary>
	public class ChannelEvents
	{
		public Task ChannelDestroyed(SocketChannel channel)
		{
			ServerList server = ServerListsManager.GetServer(((SocketGuildChannel) channel).Guild);

			//Check the server's welcome settings
			BotCheckServerSettings.CheckServerWelcomeSettings(server).GetAwaiter().GetResult();

			//Check the bot's auto voice channels
			BotCheckServerSettings.CheckServerVoiceChannels(server);
			BotCheckServerSettings.CheckServerActiveVoiceChannels(server);

			return Task.CompletedTask;
		}
	}
}
