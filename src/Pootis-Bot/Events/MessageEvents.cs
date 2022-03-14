using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Pootis_Bot.Core.Logging;
using Pootis_Bot.Core.Managers;
using Pootis_Bot.Entities;

namespace Pootis_Bot.Events
{
	/// <summary>
	/// Handles message client events
	/// </summary>
	public class MessageEvents
	{
		public async Task MessageDeleted(Cacheable<IMessage, ulong> messageCache, Cacheable<IMessageChannel, ulong> channel)
		{
			try
			{
				SocketGuild guild = ((SocketGuildChannel) channel.Value).Guild;
				ServerList server = ServerListsManager.GetServer(guild);
				if (messageCache.Id == server.RuleMessageId)
				{
					//The rule reaction will be disabled and the owner of the guild will be notified.
					server.RuleEnabled = false;

					ServerListsManager.SaveServerList();

					IDMChannel dm = await guild.Owner.CreateDMChannelAsync();
					await dm.SendMessageAsync(
						$"Your rule reaction on the Discord server **{guild.Name}** has been disabled due to the message being deleted.\n" +
						"You can enable it again after setting a new reaction message with the command `setuprulesmessage` and then enabling the feature again with `togglerulereaction`.");
				}
			}
			catch (Exception ex)
			{
				Logger.Error("An error occured while managing message deleted event! {@Exception}", ex);
			}
		}

		public async Task MessageBulkDeleted(IReadOnlyCollection<Cacheable<IMessage, ulong>> cacheable,
			Cacheable<IMessageChannel, ulong> channel)
		{
			try
			{
				SocketGuild guild = ((SocketGuildChannel) channel.Value).Guild;
				ServerList server = ServerListsManager.GetServer(guild);

				//Depending on how many message were deleted, this could take awhile. Or well I assume that, it would need to be tested
				foreach (Cacheable<IMessage, ulong> cache in cacheable)
				{
					if (cache.Id != server.RuleMessageId) continue;

					//The rule reaction will be disabled and the owner of the guild will be notified.
					server.RuleEnabled = false;

					ServerListsManager.SaveServerList();

					IDMChannel dm = await guild.Owner.CreateDMChannelAsync();
					await dm.SendMessageAsync(
						$"Your rule reaction on the Discord server **{guild.Name}** has been disabled due to the message being deleted.\n" +
						"You can enable it again after setting setting a new reaction message with the command `setuprulesmessage` and then enabling the feature again with `togglerulereaction`.");

					return;
				}
			}
			catch (Exception ex)
			{
				Logger.Error("An error occured while managing a message bulk deleted event! {@Exception}", ex);
			}
		}
	}
}