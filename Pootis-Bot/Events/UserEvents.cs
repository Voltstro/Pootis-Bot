using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord.Rest;
using Discord.WebSocket;
using Pootis_Bot.Core;
using Pootis_Bot.Entities;
using Pootis_Bot.Services.Audio;
using Pootis_Bot.Structs;

namespace Pootis_Bot.Events
{
	public class UserEvents
	{
		private readonly DiscordSocketClient _client;

		public UserEvents(DiscordSocketClient client)
		{
			_client = client;
		}

		public async Task UserJoined(SocketGuildUser user)
		{
			GlobalServerList server = ServerLists.GetServer(user.Guild);

			if (!user.IsBot)
			{
				//Pre create the user account
				UserAccounts.GetAccount(user);
				UserAccounts.SaveAccounts();

				//If the server has welcome messages enabled then we give them a warm welcome UwU
				if (server.WelcomeMessageEnabled)
				{
					//Format the message to include username and the server name
					string addUserMention = server.WelcomeMessage.Replace("[user]", user.Mention);
					string addServerName = addUserMention.Replace("[server]", user.Guild.Name);

					//Welcomes the new user with the server's message
					if (_client.GetChannel(server.WelcomeChannel) is SocketTextChannel channel)
						await channel.SendMessageAsync(addServerName);
				}
			}
		}

		public async Task UserLeft(SocketGuildUser user)
		{
			GlobalServerList server = ServerLists.GetServer(user.Guild);
			if (!user.IsBot)
			{
				//Remove server data from account
				GlobalUserAccount account = UserAccounts.GetAccount(user);
				account.Servers.Remove(account.GetOrCreateServer(user.Guild.Id));
				UserAccounts.SaveAccounts();

				if (server.WelcomeMessageEnabled)
				{
					//Format the message
					string addUserMention = server.WelcomeGoodbyeMessage.Replace("[user]", user.Username);

					//Get the welcome channel and send the message
					if (_client.GetChannel(server.WelcomeChannel) is SocketTextChannel channel)
						await channel.SendMessageAsync(addUserMention);
				}
			}
		}

		public async Task UserVoiceStateUpdated(SocketUser user, SocketVoiceState before,
			SocketVoiceState after)
		{
			GlobalServerList server = ServerLists.GetServer(((SocketGuildUser) user).Guild);

			//If we are adding an auto voice channel
			if (after.VoiceChannel != null)
			{
				VoiceChannel voiceChannel = server.GetVoiceChannel(after.VoiceChannel.Id);
				if (voiceChannel.Name != null)
				{
					RestVoiceChannel createdChannel =
						await after.VoiceChannel.Guild.CreateVoiceChannelAsync($"New {voiceChannel.Name} chat");

					int count = server.ActiveAutoVoiceChannels.Count + 1;
					await createdChannel.ModifyAsync(x =>
					{
						x.Bitrate = after.VoiceChannel.Bitrate;
						x.Name = voiceChannel.Name + " #" + count;
						x.CategoryId = after.VoiceChannel.CategoryId;
					});

					//Move the user who created the channel to the new channel
					await ((SocketGuildUser) user).ModifyAsync(x => { x.ChannelId = createdChannel.Id; });

					server.ActiveAutoVoiceChannels.Add(createdChannel.Id);
					ServerLists.SaveServerList();
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
						ServerLists.SaveServerList();
					}
			}

			//Only check channel user count if the audio services are enabled.
			if (Config.bot.IsAudioServiceEnabled)
			{
				List<GlobalServerMusicItem> toRemove = new List<GlobalServerMusicItem>();

				foreach (GlobalServerMusicItem channel in AudioService.currentChannels.Where(channel =>
					channel.AudioChannel.Users.Count == 1))
				{
					//Stop ffmpeg if it is running
					channel.FfMpeg?.Dispose();

					//Leave the audio channel
					await channel.AudioClient.StopAsync();

					await channel.StartChannel.SendMessageAsync(
						":musical_note: Left the audio channel due to there being no one there :(");

					toRemove.Add(channel);
				}

				//To avoid System.InvalidOperationException exception remove the channels after the foreach loop.
				if (toRemove.Count != 0)
					foreach (GlobalServerMusicItem channel in toRemove)
						AudioService.currentChannels.Remove(channel);
			}
		}
	}
}
