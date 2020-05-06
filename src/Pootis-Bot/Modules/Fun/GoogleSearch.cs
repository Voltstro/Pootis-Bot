using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;
using Pootis_Bot.Core;
using Pootis_Bot.Helpers;
using Pootis_Bot.Preconditions;
using Pootis_Bot.Services.Google.Search;

namespace Pootis_Bot.Modules.Fun
{
	public class GoogleSearch : ModuleBase<SocketCommandContext>
	{
		// Module Information
		// Original Author  - Creepysin
		// Description      - Searches Google
		// Contributors     - Creepysin, 

		private readonly GoogleService googleService;

		public GoogleSearch(GoogleService googleService)
		{
			this.googleService = googleService;
		}

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

		private async Task GSearch(string search, ISocketMessageChannel channel, int maxResults = 10)
		{
			EmbedBuilder embed = new EmbedBuilder();
			embed.WithTitle($"Google Search '{search}'");
			embed.WithDescription("Searching Google...");
			embed.WithFooter($"Search by {Context.User}", Context.User.GetAvatarUrl());
			embed.WithColor(FunCmdsConfig.googleColor);
			embed.WithCurrentTimestamp();

			RestUserMessage message = await channel.SendMessageAsync("", false, embed.Build());

			List<Services.Google.Search.GoogleSearch> searches = await googleService.SearchGoogle(search);

			StringBuilder description = new StringBuilder();

			int currentResult = 0;
			foreach (Services.Google.Search.GoogleSearch result in searches)
			{
				if (currentResult == maxResults) continue;

				string link = $"**[{result.ResultTitle}]({result.ResultLink})**\n{result.ResultSnippet}\n\n";

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