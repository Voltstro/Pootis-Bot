using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;
using Google.Apis.Customsearch.v1.Data;
using Pootis_Bot.Core;
using Pootis_Bot.Helpers;
using Pootis_Bot.Preconditions;
using Pootis_Bot.Services.Google;

namespace Pootis_Bot.Modules.Fun
{
	public class GoogleSearch : ModuleBase<SocketCommandContext>
	{
		// Module Information
		// Original Author  - Creepysin
		// Description      - Searches Google
		// Contributors     - Creepysin, 

		[Command("google", RunMode = RunMode.Async)]
		[Summary("Searches Google")]
		[Alias("g")]
		[Cooldown(5)]
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

			await GSearch(search, Context.Channel);
		}

		[Command("google", RunMode = RunMode.Async)]
		[Summary("Searches Google")]
		[Alias("g")]
		[Cooldown(5)]
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

			await GSearch(search, Context.Channel, maxSearchResults);
		}

		private async Task GSearch(string search, ISocketMessageChannel channel, int maxResults = 10)
		{
			EmbedBuilder embed = new EmbedBuilder();
			embed.WithTitle($"Google Search '{search}'");
			embed.WithDescription("Searching Google...");
			embed.WithFooter($"Search by {Context.User}", Context.User.GetAvatarUrl());
			embed.WithColor(FunCmdsConfig.googleColor);
			embed.WithCurrentTimestamp();

			RestUserMessage message = await channel.SendMessageAsync("", false, embed.Build());

			Search searchListResponse = GoogleService.Search(search, GetType().ToString());

			StringBuilder description = new StringBuilder();

			int currentResult = 0;
			foreach (Result result in searchListResponse.Items)
			{
				if (currentResult == maxResults) continue;

				string link = $"**[{result.Title}]({result.Link})**\n{result.Snippet}\n\n";

				if (description.Length >= 2048)
					continue;

				if (description.Length + link.Length >= 2048)
					continue;

				description.Append(link);
				currentResult += 1;
			}

			embed.WithDescription(description.ToString());
			embed.WithCurrentTimestamp();

			await MessageUtils.ModifyMessage(message, embed);
		}
	}
}