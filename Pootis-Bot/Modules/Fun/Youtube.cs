using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Google.Apis.YouTube.v3;
using Google.Apis.Services;
using Discord.Commands;
using Discord;
using Discord.WebSocket;
using Pootis_Bot.Core;

namespace Pootis_Bot.Modules.Fun
{
    public class Youtube : ModuleBase<SocketCommandContext>
    {
        readonly string ytstartLink = "https://www.youtube.com/watch?v=";
        readonly string ytChannelStart = "https://www.youtube.com/channel/";

        readonly Color youtubeColor = new Color(229, 57, 38);

        //Based off youtubes documentation
        [Command("youtube")]
        [Alias("yt")]
        public async Task CmdYoutubeSearch([Remainder] string search = "")
        {
            var server = ServerLists.GetServer(Context.Guild);

            //Check to see if the command has a permission set
            if (server.permYT != null && server.permYT != "")
            {
                var _user = Context.User as SocketGuildUser;
                var setrole = (_user as IGuildUser).Guild.Roles.FirstOrDefault(x => x.Name == server.permYT);

                if (_user.Roles.Contains(setrole))
                    await Context.Channel.SendMessageAsync("", false, YoutubeSearch(search).Build());
            }
            else
                await Context.Channel.SendMessageAsync("", false, YoutubeSearch(search).Build());
        }

        EmbedBuilder YoutubeSearch(string search)
        {
            if (search != "")
            {
                if (Config.bot.apiYoutubeKey.Trim() != "")
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
                        var searchListResponse = searchListRequest.Execute();

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
                        embed.WithColor(youtubeColor);

                        return embed;
                    }
                    catch (Exception ex)
                    {
                        Global.ColorMessage($"[{Global.TimeNow()}] An error occured while user '{Context.User}' tryied searching '{search}' on Youtube. \nError Details: \n{ex.Message}", ConsoleColor.Red);

                        EmbedBuilder embed = new EmbedBuilder
                        {
                            Title = "Youtube Search Error"
                        };
                        embed.WithDescription($"An Error Occured. It is best to tell the owner of this bot this error.\n**Error Details: ** {ex.Message}");
                        embed.WithColor(youtubeColor);

                        return embed;
                    }
                }
                else
                {
                    EmbedBuilder embed = new EmbedBuilder
                    {
                        Title = "Youtube Search Error"
                    };
                    embed.WithDescription($"We are sorry, but youtube search is disabled by the bot owner.");
                    embed.WithColor(youtubeColor);

                    return embed;
                }
            }
            else
            {
                EmbedBuilder embed = new EmbedBuilder
                {
                    Title = "Youtube Search Error"
                };
                embed.WithDescription($"Don't you want to search something?\nE.G: {Config.bot.botPrefix}youtube Dank Memes");
                embed.WithColor(youtubeColor);

                return embed;
            }
        }
    }
}