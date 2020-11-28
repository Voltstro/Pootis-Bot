using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Pootis_Bot.Config;
using Pootis_Bot.Core;
using Pootis_Bot.Helper;

namespace Pootis_Bot.Module.Basic
{
	public class BasicCommands : ModuleBase<SocketCommandContext>
	{
		private string displayName;

		public BasicCommands()
		{
			BotConfig config = Config<BotConfig>.Instance;
			displayName = config.BotName;
			config.Saved += () => displayName = config.BotName;
		}

		[Command("hello")]
		[Summary("Gets basic info about the bot")]
		public async Task Hello()
		{
			EmbedBuilder embed = new EmbedBuilder();
			embed.WithTitle("Hello!");
			embed.WithDescription($"Hello! My name is {displayName}!\n\n**__Links__**" +
			                      $"\n<:GitHub:529571722991763456> [Github Page]({Links.GitHub})" +
			                      $"\n:bookmark: [Documentation]({Links.Documentation})" +
			                      $"\n<:Discord:529572497130127360> [Voltstro Discord Server]({Links.DiscordServer})" +
			                      $"\n\nThis project is under the [MIT license]({Links.GitHub}/blob/master/LICENSE.md)");
			embed.WithFooter($"Pootis-Bot: v{VersionUtils.GetApplicationVersion()} - Discord.Net: v{VersionUtils.GetDiscordNetVersion()}");
			embed.WithColor(new Color(241, 196, 15));

			await Context.Channel.SendMessageAsync("", false, embed.Build());
		}

		[Command("ping")]
		[Summary("Gets the ping of the bot")]
		public async Task Ping()
		{
			await Context.Channel.SendMessageAsync($"Ping Pong! {Context.Client.Latency}ms.");
		}

		[Command("roll")]
		[Summary("Roles a number")]
		public async Task Roll(int min = 0, int max = 6)
		{
			if (min >= max)
			{
				await Context.Channel.SendMessageAsync("The min value cannot be the same or larger as the max value!");
				return;
			}

			await Context.Channel.SendMessageAsync($"I rolled a **{new Random().Next(min, max)}**!");
		}

		[Command("pick")]
		[Summary("Picks between a selection")]
		public async Task Pick(params string[] selection)
		{
			if (selection.Length == 0)
			{
				await Context.Channel.SendMessageAsync("You need to input a selection!");
				return;
			}

			await Context.Channel.SendMessageAsync($"I choose... {selection[new Random().Next(0, selection.Length)]}");
		}
	}
}