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
		public async Task ChannelDestroyed(SocketChannel channel)
		{
			ServerList server = ServerListsManager.GetServer(((SocketGuildChannel) channel).Guild);

			//Check the server's welcome settings
			await BotCheckServerSettings.CheckServerWelcomeSettings(server);

			//Check the bot's auto voice channels
			BotCheckServerSettings.CheckServerVoiceChannels(server);
			BotCheckServerSettings.CheckServerActiveVoiceChannels(server);

			//Check the bot's rule message channel
			await BotCheckServerSettings.CheckServerRuleMessageChannel(server);
		}
	}
}