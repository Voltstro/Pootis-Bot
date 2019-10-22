using System.Text;
using System.Threading.Tasks;
using CreepysinStudios.WikiDotNet;
using Discord;
using Discord.Commands;

namespace Pootis_Bot.Modules.Fun
{
	public class WikipediaSearch : ModuleBase<SocketCommandContext>
	{
		// Module Information
		// Original Author  - Creepysin
		// Description      - Wikipedia search
		// Contributors     - Creepysin, 

		//NOTICE ON WIKI.NET ==========
		//WIKI.NET IS STILL IN-DEVELOPMENT BY ETERNALCLICKBAIT AND ME(Creepysin)
		//YOU CAN GET WIKI.NET AT https://github.com/Creepysin-Studios/Wiki.Net

		[Command("wiki")]
		[Alias("wikipedia")]
		[Summary("Searches Wikipedia")]
		public async Task Wikipedia([Remainder] string search = "")
		{
			if (string.IsNullOrWhiteSpace(search))
			{
				await Context.Channel.SendMessageAsync("A search needs to be entered! E.G: `wiki C#`");
				return;
			}

			EmbedBuilder embed = new EmbedBuilder();
			StringBuilder sb = new StringBuilder();
			embed.WithTitle("Wikipedia Search");
			embed.WithColor(FunCmdsConfig.wikipediaSearchColor);

			//TODO: Once max search responses are added into Wiki.NET, add max search responses
			WikiSearchResponse response = WikiSearcher.Search(search);
			for (int i = 0; i < 5; i++)
			{
				WikiSearchResult result = response.SearchResults[i];
				sb.Append($"[{result.Title}]({result.Url})\n{result.Preview}\n");
			}

			embed.WithDescription(sb.ToString());
			embed.WithFooter($"Search by {Context.User.Username}", Context.User.GetAvatarUrl());
			embed.WithCurrentTimestamp();

			await Context.Channel.SendMessageAsync("", false, embed.Build());
		}
	}
}
