using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Pootis_Bot.Config;
using Pootis_Bot.Core;
using Pootis_Bot.Helper;

namespace Pootis_Bot.Module.Basic
{
	public sealed class BasicCommands : ModuleBase<SocketCommandContext>
	{
		private string displayName;

		public BasicCommands()
		{
			BotConfig config = Config<BotConfig>.Instance;
			displayName = config.BotName;
			config.Saved += () => displayName = config.BotName;
		}

		[Command("hello")]
		public async Task Hello()
		{
			EmbedBuilder embed = new EmbedBuilder();
			embed.WithTitle("Hello!");
			embed.WithDescription($"Hello! My name is {displayName}!");
			embed.WithFooter($"Pootis-Bot: v{VersionUtils.GetApplicationVersion()} - Discord.Net: v{VersionUtils.GetDiscordNetVersion()}");
			embed.WithColor(new Color(241, 196, 15));

			await Context.Channel.SendMessageAsync("", false, embed.Build());
		}
	}
}