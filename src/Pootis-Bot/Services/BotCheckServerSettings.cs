using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Pootis_Bot.Core;
using Pootis_Bot.Core.Logging;
using Pootis_Bot.Core.Managers;
using Pootis_Bot.Entities;
using Pootis_Bot.Services.Voting;
using Pootis_Bot.Structs.Server;

namespace Pootis_Bot.Services
{
	public class BotCheckServerSettings
	{
		private static DiscordSocketClient _client;

		public BotCheckServerSettings(DiscordSocketClient client)
		{
			_client = client;
		}

		/// <summary>
		/// Checks all server settings, auto vc channels, active vc channels and the welcome message
		/// </summary>
		/// <returns></returns>
		public async Task CheckConnectedServerSettings()
		{
			Logger.Log("Checking pre-connected server settings...");

			//To avoid saving possibly 100 times we will only save once if something has changed
			bool somethingChanged = false;

			List<ServerList> serversToRemove = new List<ServerList>();

			ServerList[] servers = ServerListsManager.GetServers();
			foreach (ServerList server in servers)
			{
				//The bot is not longer in this guild, remove it from the server settings
				if (_client.GetGuild(server.GuildId) == null)
				{
					somethingChanged = true;
					serversToRemove.Add(server);
					continue;
				}

				await CheckServerWelcomeSettings(server);
				await CheckServerRuleMessageChannel(server);

				CheckServerVoiceChannels(server);
				CheckServerActiveVoiceChannels(server);
				CheckServerPerms(server);

				//Start up all votes
				foreach (Vote serverVote in server.Votes)
				{
#pragma warning disable 4014
					Task.Run(() => VotingService.RunVote(serverVote, _client.GetGuild(server.GuildId)));
#pragma warning restore 4014
				}
			}

			//Like all the other ones, we remove all the unnecessary servers after to avoid System.InvalidOperationException
			foreach (ServerList toRemove in serversToRemove)
			{
				Logger.Log($"The bot is not longer in the {toRemove.GuildId}, Removing server settings...");
				ServerListsManager.RemoveServer(toRemove);
			}

			//If a server was updated then save the ServerList.json file
			if (somethingChanged)
				ServerListsManager.SaveServerList();

			Logger.Log("Checked all server settings.");
		}

		/// <summary>
		/// Checks the server's welcome settings
		/// </summary>
		/// <param name="server"></param>
		/// <returns></returns>
		public static async Task CheckServerWelcomeSettings(ServerList server)
		{
			//If the welcome channel doesn't exist and welcome/goodbye message is enabled, then disable it and tell the owner
			if (_client.GetChannel(server.WelcomeChannelId) == null && server.WelcomeMessageEnabled)
			{
				SocketGuild guild = _client.GetGuild(server.GuildId);
				IDMChannel ownerDm = await _client.GetGuild(server.GuildId).Owner.GetOrCreateDMChannelAsync();

				await ownerDm.SendMessageAsync(
					$"{guild.Owner.Mention}, your server **{guild.Name}** welcome channel has been disabled due to that it no longer exist since the last bot up time.\n" +
					$"You can enable it again with `{Global.BotPrefix}setupwelcomemessage` command and your existing message should stay.");

				server.WelcomeMessageEnabled = false;
				server.WelcomeChannelId = 0;

				ServerListsManager.SaveServerList();
			}
		}

		/// <summary>
		/// Checks all the bot's auto voice channels
		/// </summary>
		/// <param name="server"></param>
		public static void CheckServerVoiceChannels(ServerList server)
		{
			//Get all the voice channels that have been deleted
			List<ServerVoiceChannel> autoVcChannelsToDelete = (from autoVoiceChannel in server.AutoVoiceChannels
				let vcChannel = _client.GetGuild(server.GuildId).GetVoiceChannel(autoVoiceChannel.Id)
				where vcChannel == null
				select autoVoiceChannel).ToList();

			foreach (ServerVoiceChannel voiceChannel in autoVcChannelsToDelete)
				server.AutoVoiceChannels.Remove(voiceChannel);

			ServerListsManager.SaveServerList();
		}

		/// <summary>
		/// Checks all the bot's active auto voice channels
		/// </summary>
		/// <param name="server"></param>
		public static void CheckServerActiveVoiceChannels(ServerList server)
		{
			//Get all the active voice channels that have been deleted, or have no one in it
			List<ulong> autoVcChannelsToDelete = (from serverActiveAutoVoiceChannel in server.ActiveAutoVoiceChannels
				let vcChannel = _client.GetGuild(server.GuildId).GetVoiceChannel(serverActiveAutoVoiceChannel)
				where vcChannel == null
				select serverActiveAutoVoiceChannel).ToList();

			foreach (ulong voiceChannel in autoVcChannelsToDelete) server.ActiveAutoVoiceChannels.Remove(voiceChannel);

			List<ulong> autoVcWithNoUsers = (from activeAutoVoiceChannel in server.ActiveAutoVoiceChannels
				let vcChannel = _client.GetGuild(server.GuildId).GetVoiceChannel(activeAutoVoiceChannel)
				where vcChannel.Users.Count == 0
				select activeAutoVoiceChannel).ToList();

			foreach (ulong autoVoiceChannel in autoVcWithNoUsers)
			{
				_client.GetGuild(server.GuildId).GetVoiceChannel(autoVoiceChannel).DeleteAsync();
				server.ActiveAutoVoiceChannels.Remove(autoVoiceChannel);
			}

			ServerListsManager.SaveServerList();
		}

		/// <summary>
		/// Checks all server permission roles to see if they still exist
		/// </summary>
		/// <param name="server"></param>
		public static void CheckServerPerms(ServerList server)
		{
			foreach (ServerList.CommandPermission perm in server.CommandPermissions)
			{
				List<ulong> rolesToRemove =
					perm.Roles.Where(role => _client.GetGuild(server.GuildId).GetRole(role) == null).ToList();

				foreach (ulong roleToRemove in rolesToRemove) perm.Roles.Remove(roleToRemove);
			}

			PermissionService.RemoveAllCommandsWithNoRoles(server);

			ServerListsManager.SaveServerList();
		}

		public static async Task CheckServerRuleMessageChannel(ServerList server)
		{
			if (!server.RuleEnabled) return;

			if (_client.GetGuild(server.GuildId).GetChannel(server.RuleMessageChannelId) == null)
			{
				//The rule reaction will be disabled and the owner of the guild will be notified.
				server.RuleEnabled = false;

				ServerListsManager.SaveServerList();

				IDMChannel dm = await _client.GetGuild(server.GuildId).Owner.GetOrCreateDMChannelAsync();
				await dm.SendMessageAsync(
					$"Your rule reaction on the Discord server **{_client.GetGuild(server.GuildId).Name}** has been disabled due to the message being deleted.\n" +
					"You can enable it again after setting a new reaction message with the command `setuprulesmessage` and then enabling the feature again with `togglerulereaction`.");
			}
		}
	}
}