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
using Pootis_Bot.Services.Google.YouTube;

namespace Pootis_Bot.Modules.Fun
{
	public class YoutubeSearch : ModuleBase<SocketCommandContext>
	{
		// Module Information
		// Original Author  - Voltstro
		// Description      - Searches YouTube
		// Contributors     - Voltstro, 

		private readonly YouTubeService youtubeService;

		public YoutubeSearch(YouTubeService ytService)
		{
			youtubeService = ytService;
		}

		[Command("youtube", RunMode = RunMode.Async)]
		[Summary("Searches Youtube")]
		[Alias("yt")]
		[Cooldown(5)]
		[RequireBotPermission(GuildPermission.EmbedLinks)]
		public async Task Youtube([Remainder] string search = "")
		{
			//YouTube service has been disabled
			if (!Config.bot.Apis.YouTubeService)
			{
				await Context.Channel.SendMessageAsync("YouTube search is disabled by the bot owner.");
				return;
			}

			//No or blank input
			if (string.IsNullOrWhiteSpace(search))
			{
				await Context.Channel.SendMessageAsync("The search input cannot be blank!");
				return;
			}

			await YtSearch(search, Context.Channel);
		}

		private async Task YtSearch(string search, ISocketMessageChannel channel)
		{
			EmbedBuilder embed = new EmbedBuilder();
			embed.WithTitle($"YouTube Search '{search}'");
			embed.WithDescription("Searching YouTube...");
			embed.WithFooter($"Search by {Context.User}", Context.User.GetAvatarUrl());
			embed.WithCurrentTimestamp();
			embed.WithColor(FunCmdsConfig.youtubeColor);

			RestUserMessage message = await channel.SendMessageAsync("", false, embed.Build());

			//Search Youtube
			IList<YouTubeVideo> searchResponse = await youtubeService.SearchForYouTube(search);

			StringBuilder videos = new StringBuilder();
			if (searchResponse != null)
				foreach (YouTubeVideo video in searchResponse)
					videos.Append(
						$"**[{video.VideoTitle.RemoveIllegalChars()}]({FunCmdsConfig.ytChannelStart}{video.VideoId})**\n{video.VideoDescription}\n\n");

			embed.WithDescription($"**Videos**\n{videos}");
			embed.WithCurrentTimestamp();

			await MessageUtils.ModifyMessage(message, embed);
		}
	}
}