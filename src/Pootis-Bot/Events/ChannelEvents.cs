using System;
using System.Threading.Tasks;
using Discord.WebSocket;
using Pootis_Bot.Core.Logging;
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
			try
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
			catch (Exception ex)
			{
				Logger.Error("An error occured while managing channel destroyed event! {@Exception}", ex);
			}
		}
	}
}