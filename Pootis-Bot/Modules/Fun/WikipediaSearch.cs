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

		//This use a library called Wiki.Net
		//It was developed by my good friend EternalClickbait and partially by me(Creepysin).
		//You can get it here: https://github.com/Creepysin-Studios/Wiki.Net

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

			await Context.Channel.SendMessageAsync("", false, WikiSearch(search));
		}

		[Command("wiki")]
		[Alias("wikipedia")]
		[Summary("Searches Wikipedia")]
		public async Task Wikipedia(int maxSearchResults = 15, [Remainder] string search = "")
		{
			if (string.IsNullOrWhiteSpace(search))
			{
				await Context.Channel.SendMessageAsync("A search needs to be entered! E.G: `wiki C#`");
				return;
			}

			if (maxSearchResults > FunCmdsConfig.wikipediaMaxSearches)
			{
				await Context.Channel.SendMessageAsync(
					$"The max search amount you have put in is too high! It has to be below {FunCmdsConfig.wikipediaMaxSearches}.");
				return;
			}

			await Context.Channel.SendMessageAsync("", false, WikiSearch(search, maxSearchResults));
		}

		private Embed WikiSearch(string search, int maxSearch = 10)
		{
			EmbedBuilder embed = new EmbedBuilder();

			StringBuilder sb = new StringBuilder();
			embed.WithTitle($"Wikipedia Search '{search}'");
			embed.WithColor(FunCmdsConfig.wikipediaSearchColor);

			WikiSearchResponse response = WikiSearcher.Search(search, new WikiSearchSettings
			{
				ResultLimit = maxSearch
			});

			foreach (WikiSearchResult result in response.Query.SearchResults)
			{
				string message =
					$"**[{result.Title}]({result.ConstantUrl})** (Words: {result.WordCount})\n{result.Preview}\n\n";

				//There is a character limit of 2048, so lets make sure we don't hit that
				if (sb.Length >= 2048)
				{
					continue;
				}

				if (sb.Length + message.Length >= 2048)
				{
					continue;
				}

				sb.Append(message);
			}

			embed.WithDescription(sb.ToString());
			embed.WithFooter($"Search by {Context.User}", Context.User.GetAvatarUrl());
			embed.WithCurrentTimestamp();

			return embed.Build();
		}
	}
}
