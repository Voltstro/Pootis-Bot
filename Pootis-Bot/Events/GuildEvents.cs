﻿using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Pootis_Bot.Core;

namespace Pootis_Bot.Events
{
	public class GuildEvents
	{
		public async Task JoinedNewServer(SocketGuild guild)
		{
			//Add the new server to the server list
			ServerLists.GetServer(guild);
			ServerLists.SaveServerList();

			EmbedBuilder embed = new EmbedBuilder();
			embed.WithTitle("Hey, thanks for adding me to your server.");
			embed.WithDescription("Hello! My name is " + Global.BotName + "!\n\n**__Links__**" +
			                      $"\n:computer: [Commands]({Global.websiteCommands})" +
			                      $"\n<:GitHub:529571722991763456> [Github Page]({Global.githubPage})" +
			                      $"\n:bookmark: [Documentation]({Global.websiteHome})" +
			                      $"\n<:Discord:529572497130127360> [Creepysin Development Server]({Global.discordServers[1]})" +
			                      $"\n<:Discord:529572497130127360> [Creepysin Server]({Global.discordServers[0]})" +
			                      "\n\nIf you have any issues the best place to ask for assistance is on the Creepysin Server!");
			embed.WithColor(new Color(241, 196, 15));

			//Send a message to the server's default channel with the hello message
			await guild.DefaultChannel.SendMessageAsync("", false, embed.Build());

			//Send a message to Discord server's owner about setting up the bot
			IDMChannel owner = await guild.Owner.GetOrCreateDMChannelAsync();
			await owner.SendMessageAsync(
				$"Thanks for using {Global.BotName}! Check out {Global.websiteServerSetup} on how to setup {Global.BotName} for your server.");
		}
	}
}