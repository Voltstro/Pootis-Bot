using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;
using Google.Apis.YouTube.v3.Data;
using Pootis_Bot.Core;
using Pootis_Bot.Preconditions;
using Pootis_Bot.Services.Audio;
using Pootis_Bot.Services.Google;
using SearchResult = Google.Apis.YouTube.v3.Data.SearchResult;

namespace Pootis_Bot.Modules.Fun
{
	public class YoutubeSearch : ModuleBase<SocketCommandContext>
	{
		// Module Information
		// Original Author  - Creepysin
		// Description      - Searches YouTube
		// Contributors     - Creepysin, 

		[Command("youtube")]
		[Summary("Searches Youtube")]
		[Alias("yt")]
		[Cooldown(5)]
		[RequireBotPermission(GuildPermission.EmbedLinks)]
		public async Task Youtube([Remainder] string search = "")
		{
			if (string.IsNullOrWhiteSpace(Config.bot.Apis.ApiYoutubeKey))
			{
				await Context.Channel.SendMessageAsync("YouTube search is disabled by the bot owner.");
				return;
			}

			if (string.IsNullOrWhiteSpace(search))
			{
				await Context.Channel.SendMessageAsync("The search input cannot be blank!");
				return;
			}

			await YtSearch(search, Context.Channel);

			//await Context.Channel.SendMessageAsync("", false, YtSearch(search));
		}

		[Command("youtube")]
		[Summary("Searches Youtube")]
		[Alias("yt")]
		[Cooldown(5)]
		[RequireBotPermission(GuildPermission.EmbedLinks)]
		public async Task Youtube(int maxSearchResults = 6, [Remainder] string search = "")
		{
			if (string.IsNullOrWhiteSpace(Config.bot.Apis.ApiYoutubeKey))
			{
				await Context.Channel.SendMessageAsync("YouTube search is disabled by the bot owner.");
				return;
			}

			if (string.IsNullOrWhiteSpace(search))
			{
				await Context.Channel.SendMessageAsync("The search input cannot be blank!");
				return;
			}

			if (maxSearchResults > FunCmdsConfig.youtubeMaxSearches)
			{
				await Context.Channel.SendMessageAsync(
					$"The max search amount you have put in is too high! It has to be below {FunCmdsConfig.youtubeMaxSearches}.");
				return;
			}

			await YtSearch(search, Context.Channel, maxSearchResults);
		}

		private async Task YtSearch(string search, ISocketMessageChannel channel, int maxSearch = 6)
		{
			EmbedBuilder embed = new EmbedBuilder();
			embed.WithTitle($"YouTube Search '{search}'");
			embed.WithDescription("Searching YouTube...");
			embed.WithFooter($"Search by {Context.User}", Context.User.GetAvatarUrl());
			embed.WithCurrentTimestamp();
			embed.WithColor(FunCmdsConfig.youtubeColor);

			RestUserMessage message = await channel.SendMessageAsync("", false, embed.Build());

			//Search Youtube
			SearchListResponse searchListResponse = YoutubeService.Search(search, GetType().ToString(), maxSearch);

			StringBuilder videos = new StringBuilder();
			StringBuilder channels = new StringBuilder();

			if (searchListResponse != null)
			{
				foreach (SearchResult result in searchListResponse.Items)
				{
					switch (result.Id.Kind)
					{
						case "youtube#video":
							videos.Append(
								$"**[{AudioCheckService.RemovedNotAllowedChars(result.Snippet.Title)}]({FunCmdsConfig.ytStartLink}{result.Id.VideoId})**\n{result.Snippet.Description}\n\n");
							break;
						case "youtube#channel":
							channels.Append(
								$"**[{AudioCheckService.RemovedNotAllowedChars(result.Snippet.Title)}]({FunCmdsConfig.ytChannelStart}{result.Id.ChannelId})**\n{result.Snippet.Description}\n\n");
							break;
					}
				}
			}

			embed.WithDescription($"**Videos**\n{videos}\n\n**Channels**\n{channels}");
			embed.WithCurrentTimestamp();

			await message.ModifyAsync(x => { x.Embed = embed.Build(); });
		}
	}
}