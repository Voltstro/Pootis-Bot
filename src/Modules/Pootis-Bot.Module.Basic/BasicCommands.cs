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
			embed.WithDescription($"Hello! My name is {displayName}!\n\n**__Links__**" +
			                      $"\n<:GitHub:529571722991763456> [Github Page]({Links.GitHub})" +
			                      $"\n:bookmark: [Documentation]({Links.Documentation})" +
			                      $"\n<:Discord:529572497130127360> [Voltstro Discord Server]({Links.DiscordServer})" +
			                      $"\n\nThis project is under the [MIT license]({Links.GitHub}/blob/master/LICENSE.md)");
			embed.WithFooter($"Pootis-Bot: v{VersionUtils.GetApplicationVersion()} - Discord.Net: v{VersionUtils.GetDiscordNetVersion()}");
			embed.WithColor(new Color(241, 196, 15));

			await Context.Channel.SendMessageAsync("", false, embed.Build());
		}
	}
}