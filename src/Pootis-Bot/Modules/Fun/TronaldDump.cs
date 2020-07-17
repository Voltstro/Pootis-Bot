using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Pootis_Bot.Preconditions;
using Pootis_Bot.Services.Fun;

namespace Pootis_Bot.Modules.Fun
{
	public class TronaldDump : ModuleBase<SocketCommandContext>
	{
		// Module Information
		// Original Author  - Voltstro
		// Description      - Uses the tronaldump api to get Tronald Dump quotes
		// Contributors     - Voltstro, 

		private readonly string trumpImageUrl = "https://assets.tronalddump.io/img/tronalddump_850x850.png";

		[Command("tronald", RunMode = RunMode.Async)]
		[Summary("Search Donald Trump quotes")]
		[Alias("tronalddump", "dump", "donald", "donaldtrump", "trump")]
		[Cooldown(5)]
		public async Task Tronald([Remainder] string subCmd = "random")
		{
			EmbedBuilder embed = new EmbedBuilder();
			embed.WithThumbnailUrl(trumpImageUrl);
			embed.WithColor(FunCmdsConfig.trumpQuoteColor);

			if (subCmd == "random")
			{
				embed.WithTitle("Random Trump Quote");
				embed.WithDescription(TronaldDumpService.GetRandomQuote());
			}
			else
			{
				embed.WithTitle("Donald Trump Quote Search");
				embed.WithDescription(TronaldDumpService.GetQuote(subCmd));
			}

			await Context.Channel.SendMessageAsync("", false, embed.Build());
		}
	}
}