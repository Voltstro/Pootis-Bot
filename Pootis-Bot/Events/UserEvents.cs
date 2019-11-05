using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord.Rest;
using Discord.WebSocket;
using Pootis_Bot.Core;
using Pootis_Bot.Core.Managers;
using Pootis_Bot.Entities;
using Pootis_Bot.Services.Audio;
using Pootis_Bot.Structs.Server;

namespace Pootis_Bot.Events
{
	/// <summary>
	/// Handles user client events
	/// </summary>
	public class UserEvents
	{
		private readonly DiscordSocketClient _client;

		public UserEvents(DiscordSocketClient client)
		{
			_client = client;
		}

		public async Task UserJoined(SocketGuildUser user)
		{
			ServerList server = ServerListsManager.GetServer(user.Guild);

			if (!user.IsBot)
			{
				//Pre create the user account
				UserAccountsManager.GetAccount(user);
				UserAccountsManager.SaveAccounts();

				//If the server has welcome messages enabled then we give them a warm welcome UwU
				if (server.WelcomeMessageEnabled)
				{
					//Format the message to include username and the server name
					string addUserMention = server.WelcomeMessage.Replace("[user]", user.Mention);
					string addServerName = addUserMention.Replace("[server]", user.Guild.Name);

					//Welcomes the new user with the server's message
					if (_client.GetChannel(server.WelcomeChannelId) is SocketTextChannel channel)
						await channel.SendMessageAsync(addServerName);
				}
			}
		}

		public async Task UserLeft(SocketGuildUser user)
		{
			ServerList server = ServerListsManager.GetServer(user.Guild);
			if (!user.IsBot)
			{
				//Remove server data from account
				UserAccount account = UserAccountsManager.GetAccount(user);
				account.Servers.Remove(account.GetOrCreateServer(user.Guild.Id));
				UserAccountsManager.SaveAccounts();

				if (server.WelcomeMessageEnabled)
				{
					//Format the message
					string addUserMention = server.WelcomeGoodbyeMessage.Replace("[user]", user.Username);

					//Get the welcome channel and send the message
					if (_client.GetChannel(server.WelcomeChannelId) is SocketTextChannel channel)
						await channel.SendMessageAsync(addUserMention);
				}
			}
		}

		public async Task UserVoiceStateUpdated(SocketUser user, SocketVoiceState before,
			SocketVoiceState after)
		{
			ServerList server = ServerListsManager.GetServer(((SocketGuildUser) user).Guild);

			//If we are adding an auto voice channel
			if (after.VoiceChannel != null)
			{
				ServerVoiceChannel voiceChannel = server.GetAutoVoiceChannel(after.VoiceChannel.Id);
				if (voiceChannel.Name != null)
				{
					RestVoiceChannel createdChannel =
						await after.VoiceChannel.Guild.CreateVoiceChannelAsync($"{voiceChannel.Name} #" + server.ActiveAutoVoiceChannels.Count + 1, x =>
						{
							x.CategoryId = after.VoiceChannel.CategoryId;
							x.Bitrate = after.VoiceChannel.Bitrate;
							x.Position = after.VoiceChannel.Position + 1;
						});

					if(createdChannel.CategoryId != null)
						await createdChannel.SyncPermissionsAsync();

					//Move the user who created the channel to the new channel
					await ((SocketGuildUser) user).ModifyAsync(x => { x.ChannelId = createdChannel.Id; });

					server.ActiveAutoVoiceChannels.Add(createdChannel.Id);
					ServerListsManager.SaveServerList();
				}
			}

			//If we are removing an auto voice channel
			if (before.VoiceChannel != null)
			{
				ulong activeChannel = server.GetActiveVoiceChannel(before.VoiceChannel.Id);
				if (activeChannel != 0)
					//There are no user on the active auto voice channel
					if (before.VoiceChannel.Users.Count == 0)
					{
						await before.VoiceChannel.DeleteAsync();
						server.ActiveAutoVoiceChannels.Remove(before.VoiceChannel.Id);
						ServerListsManager.SaveServerList();
					}
			}

			//Only check channel user count if the audio services are enabled.
			if (Config.bot.AudioSettings.AudioServicesEnabled)
			{
				List<ServerMusicItem> toRemove = new List<ServerMusicItem>();

				foreach (ServerMusicItem channel in AudioService.currentChannels.Where(channel =>
					channel.AudioChannel.Users.Count == 1))
				{
					channel.IsExit = true;

					if (channel.FfMpeg != null)
					{
						channel.IsExit = true;
						channel.FfMpeg.Kill();
						channel.FfMpeg.Dispose();
					}

					//Just wait a moment
					await Task.Delay(100);

					await channel.AudioClient.StopAsync();

					channel.IsPlaying = false;

					await channel.StartChannel.SendMessageAsync(
						":musical_note: Left the audio channel due to there being no one there :(");

					toRemove.Add(channel);
				}

				//To avoid System.InvalidOperationException exception remove the channels after the foreach loop.
				foreach (ServerMusicItem channel in toRemove)
						AudioService.currentChannels.Remove(channel);
			}
		}

		public Task UserBanned(SocketUser user, SocketGuild guild)
		{
			//We remove the user's server data from the user account ONLY if they are banned since their chance of coming back if very low.
			//If the data was deleted when they left/kicked they would also loose their warnings. I am sure you can see what the issue would be if we allowed that.

			UserAccount userAccount = UserAccountsManager.GetAccount((SocketGuildUser)user);
			userAccount.Servers.Remove(userAccount.GetOrCreateServer(guild.Id));

			UserAccountsManager.SaveAccounts();

			return Task.CompletedTask;
		}
	}
}
