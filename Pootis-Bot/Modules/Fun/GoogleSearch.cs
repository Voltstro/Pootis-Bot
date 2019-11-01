using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Google.Apis.Customsearch.v1.Data;
using Pootis_Bot.Core;
using Pootis_Bot.Services.Google;

namespace Pootis_Bot.Modules.Fun
{
	public class GoogleSearch : ModuleBase<SocketCommandContext>
	{
		// Module Information
		// Original Author  - Creepysin
		// Description      - Searches Google
		// Contributors     - Creepysin, 

		[Command("google")]
		[Summary("Searches Google")]
		[Alias("g")]
		[RequireBotPermission(GuildPermission.EmbedLinks)]
		public async Task Google([Remainder] string search = "")
		{
			if (string.IsNullOrWhiteSpace(Config.bot.Apis.ApiGoogleSearchKey) ||
			    string.IsNullOrWhiteSpace(Config.bot.Apis.GoogleSearchEngineId))
			{
				await Context.Channel.SendMessageAsync("Google search is disabled by the bot owner.");
				return;
			}

			if (string.IsNullOrEmpty(search))
			{
				await Context.Channel.SendMessageAsync("The search input cannot be blank!");
				return;
			}

			await Context.Channel.SendMessageAsync("", false, GSearch(search));
		}

		[Command("google")]
		[Summary("Searches Google")]
		[Alias("g")]
		[RequireBotPermission(GuildPermission.EmbedLinks)]
		public async Task Google(int maxSearchResults = 10, [Remainder] string search = "")
		{
			if (string.IsNullOrWhiteSpace(Config.bot.Apis.ApiGoogleSearchKey) ||
			    string.IsNullOrWhiteSpace(Config.bot.Apis.GoogleSearchEngineId))
			{
				await Context.Channel.SendMessageAsync("Google search is disabled by the bot owner.");
				return;
			}

			if (string.IsNullOrEmpty(search))
			{
				await Context.Channel.SendMessageAsync("The search input cannot be blank!");
				return;
			}

			if (maxSearchResults > FunCmdsConfig.googleMaxSearches)
			{
				await Context.Channel.SendMessageAsync(
					$"The max search amount you have put in is too high! It has to be below {FunCmdsConfig.googleMaxSearches}.");
				return;
			}

			await Context.Channel.SendMessageAsync("", false, GSearch(search, maxSearchResults));
		}

		private Embed GSearch(string search, int maxResults = 10)
		{
			Search searchListResponse = GoogleService.Search(search, GetType().ToString());

			StringBuilder description = new StringBuilder();

			int currentResult = 0;
			foreach (Result result in searchListResponse.Items)
			{
				if (currentResult == maxResults) continue;

				string message = $"**[{result.Title}]({result.Link})**\n{result.Snippet}\n\n";

				if (description.Length >= 2048)
					continue;

				if (description.Length + message.Length >= 2048)
					continue;

				description.Append(message);
				currentResult += 1;
			}

			EmbedBuilder embed = new EmbedBuilder();
			embed.WithTitle($"Google Search '{search}'");
			embed.WithDescription(description.ToString());
			embed.WithFooter($"Search by {Context.User}", Context.User.GetAvatarUrl());
			embed.WithColor(FunCmdsConfig.googleColor);
			embed.WithCurrentTimestamp();

			return embed.Build();
		}
	}
}