using System;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Pootis_Bot.Core;
using Pootis_Bot.Core.Logging;
using Pootis_Bot.Core.Managers;
using Pootis_Bot.Entities;

namespace Pootis_Bot.Events
{
	/// <summary>
	/// Handles guild client events
	/// </summary>
	public class GuildEvents
	{
		public async Task JoinedNewServer(SocketGuild guild)
		{
			try
			{
				//Add the new server to the server list
				ServerListsManager.GetServer(guild);
				ServerListsManager.SaveServerList();

				EmbedBuilder embed = new EmbedBuilder();
				embed.WithTitle("Hey, thanks for adding me to your server.");
				embed.WithDescription($"Hello! My name is {Global.BotName}!\n\n**__Links__**" +
				                      $"\n:computer: [Commands]({Global.websiteCommands})" +
				                      $"\n<:GitHub:529571722991763456> [Github Page]({Global.githubPage})" +
				                      $"\n:bookmark: [Documentation]({Global.websiteHome})" +
				                      $"\n<:Discord:529572497130127360> [Creepysin Server]({Global.discordServers[0]})" +
				                      "\n\nIf you have any issues the best place to ask for assistance is on the Creepysin Server!");
				embed.WithColor(new Color(241, 196, 15));

				//Log that the bot joined a new guild, if enabled
				if (Config.bot.ReportGuildEventsToOwner)
					await Global.BotOwner.SendMessageAsync($"LOG: Joined guild {guild.Name}({guild.Id})");

				//First, check to make sure the default channel isn't null
				if (guild.DefaultChannel != null)
					//Send a message to the server's default channel with the hello message
					await guild.DefaultChannel.SendMessageAsync("", false, embed.Build());
			}
			catch (Exception ex)
			{
#if DEBUG
				Logger.Log(ex.ToString(), LogVerbosity.Error);
#else
				Logger.Log(ex.Message, LogVerbosity.Error);
#endif
			}
		}

		public async Task LeftServer(SocketGuild guild)
		{
			try
			{
				//Remove the server settings from the serverlist.json file
				ServerList server = ServerListsManager.GetServer(guild);
				ServerListsManager.RemoveServer(server);

				ServerListsManager.SaveServerList();

				//Log that the bot left a guild, if enabled
				if (Config.bot.ReportGuildEventsToOwner)
					await Global.BotOwner.SendMessageAsync($"LOG: Left guild {guild.Name}({guild.Id})");
			}
			catch (Exception ex)
			{
#if DEBUG
				Logger.Log(ex.ToString(), LogVerbosity.Error);
#else
				Logger.Log(ex.Message, LogVerbosity.Error);
#endif
			}
		}
	}
}