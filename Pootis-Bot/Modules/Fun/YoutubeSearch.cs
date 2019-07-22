using System;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Pootis_Bot.Core;
using Pootis_Bot.Services.Google;

namespace Pootis_Bot.Modules.Fun
{
    public class YoutubeSearch : ModuleBase<SocketCommandContext>
    {
        // Module Infomation
        // Orginal Author   - Creepysin
        // Description      - Searches YouTube
        // Contributors     - Creepysin, 

        [Command("youtube")]
        [Summary("Searches Youtube")]
        [Alias("yt")]
        [RequireBotPermission(GuildPermission.EmbedLinks)]
        public async Task CmdYoutubeSearch([Remainder] string search = "")
        {
            if (string.IsNullOrWhiteSpace(Config.bot.apis.apiYoutubeKey))
            {
                await Context.Channel.SendMessageAsync("YouTube search is disabled by the bot owner.");
                return;
            }

            //Search Youtube
            var searchListResponse = YoutubeService.Search(search, this.GetType().ToString(), 6);

            StringBuilder videos = new StringBuilder();
            StringBuilder channels = new StringBuilder();

            if (searchListResponse == null)
                Console.WriteLine("Is null");

            foreach (var result in searchListResponse.Items)
            {
                if(result.Id.Kind == "youtube#video")
                    videos.Append($"[{result.Snippet.Title}]({FunCmdsConfig.ytstartLink}{result.Id.VideoId})\n{result.Snippet.Description}\n");
                if(result.Id.Kind == "youtube#channel")
                    channels.Append($"[{result.Snippet.Title}]({FunCmdsConfig.ytstartLink}{result.Id.ChannelId})\n{result.Snippet.Description}\n");
            }

            EmbedBuilder embed = new EmbedBuilder
            {
                Title = $"Youtube Search '{search}'"
            };
            embed.WithDescription($"**Videos**\n{videos.ToString()}\n\n**Channels**\n{channels.ToString()}");
            embed.WithFooter($"Search by {Context.User} @ ", Context.User.GetAvatarUrl());
            embed.WithCurrentTimestamp();
            embed.WithColor(FunCmdsConfig.youtubeColor);

            await Context.Channel.SendMessageAsync("", false, embed.Build());
        }
    }
}
