using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Google.Apis.YouTube.v3;
using Google.Apis.Services;
using Discord;
using System.Linq;

namespace Pootis_Bot.Modules.Fun
{
    public class Youtube : ModuleBase<SocketCommandContext>
    {
        readonly string ytstartLink = "https://www.youtube.com/watch?v=";
        readonly string ytChannelStart = "https://www.youtube.com/channel/";

        //Based off youtubes documentation
        [Command("youtube")]
        [Alias("yt")]
        public async Task YoutubeSearch([Remainder] string search = "")
        {
            if(search != "")
            {
                try
                {
                    var youtube = new YouTubeService(new BaseClientService.Initializer()
                    {
                        ApiKey = Config.bot.apiYoutubeKey,
                        ApplicationName = this.GetType().ToString()
                    });

                    var searchListRequest = youtube.Search.List("snippet");
                    searchListRequest.Q = search; // Replace with your search term.
                    searchListRequest.MaxResults = 10;

                    // Call the search.list method to retrieve results matching the specified query term.
                    var searchListResponse = await searchListRequest.ExecuteAsync();

                    List<string> videos = new List<string>();
                    List<string> channels = new List<string>();

                    // Add each result to the appropriate list, and then display the lists of
                    // matching videos and channels.
                    foreach (var searchResult in searchListResponse.Items)
                    {
                        switch (searchResult.Id.Kind)
                        {
                            case "youtube#video":
                                videos.Add(String.Format($"{searchResult.Snippet.Title}\n({ytstartLink + searchResult.Id.VideoId})"));
                                break;

                            case "youtube#channel":
                                channels.Add(String.Format($"{searchResult.Snippet.Title}\n({ytChannelStart + searchResult.Id.ChannelId})"));
                                break;
                        }
                    }

                    string _videos = String.Format("**Videos:**\n{0}\n", string.Join("\n", videos));
                    string _channels = String.Format("**Channels:**\n{0}\n", string.Join("\n", channels));

                    EmbedBuilder embed = new EmbedBuilder();
                    EmbedFooterBuilder embedfoot = new EmbedFooterBuilder();
                    embed.Title = $"Youtube Search '{search}'";
                    embed.WithDescription($"{_videos} \n {_channels}");

                    embedfoot.WithIconUrl(Context.User.GetAvatarUrl());
                    embedfoot.WithText("Commanded issued by " + Context.User); 

                    embed.WithFooter(embedfoot);
                    embed.WithColor(new Color(229, 57, 38));
                    await Context.Channel.SendMessageAsync("", false, embed);
                }
                catch (Exception ex)
                {
                    EmbedBuilder embed = new EmbedBuilder
                    {
                        Title = "Youtube Search Error"
                    };
                    embed.WithDescription($"An Error Occured. It is best to tell the owner of this bot this error.\n**Error Details: ** {ex.Message}");
                    embed.WithColor(new Color(229, 57, 38));
                    await Context.Channel.SendMessageAsync("", false, embed);
                    return;
                }         
            }
            else
            {
                EmbedBuilder embed = new EmbedBuilder
                {
                    Title = "Youtube Search Error"
                };
                embed.WithDescription($"Don't you want to search something?\nE.G: {Config.bot.botPrefix}youtube Dank Memes");
                embed.WithColor(new Color(229, 57, 38));
                await Context.Channel.SendMessageAsync("", false, embed);
            }        
        }
    }
}