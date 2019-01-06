using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Google.Apis.Customsearch.v1;
using Google.Apis.Services;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Pootis_Bot.Core;

namespace Pootis_Bot.Modules.Fun
{
    /*  
     *  Name: Google Search
     *  Command(s): google [Search]
     *  Set Permission Command(s): permgoogle [Role Name]
     *  Author(s): Creepysin
     */ 

    public class Google : ModuleBase<SocketCommandContext>
    {
        readonly Color googleColor = new Color(53, 169, 84);

        [Command("google")]
        [Alias("g")]
        public async Task CmdGoogleSearch([Remainder]string search = "")
        {
            var server = ServerLists.GetServer(Context.Guild);

            //Check to see if the command has a permission set
            if (server.permGoogle != null && server.permGoogle != "")
            {
                var _user = Context.User as SocketGuildUser;
                var setrole = (_user as IGuildUser).Guild.Roles.FirstOrDefault(x => x.Name == server.permGoogle);

                if(_user.Roles.Contains(setrole))
                    await Context.Channel.SendMessageAsync("", false, GoogleSearch(search).Build());
            }
            else
                await Context.Channel.SendMessageAsync("", false, GoogleSearch(search).Build());
        }

        EmbedBuilder GoogleSearch(string search)
        {
            if (search != "")
            {
                if (Config.bot.apiGoogleSearchKey.Trim() != "" || Config.bot.googleSearchEngineID.Trim() == "")
                {
                    try
                    {
                        var google = new CustomsearchService(new BaseClientService.Initializer()
                        {
                            ApiKey = Config.bot.apiGoogleSearchKey,
                            ApplicationName = this.GetType().ToString()
                        });

                        var searchListRequest = google.Cse.List(search);
                        searchListRequest.Cx = Config.bot.googleSearchEngineID;

                        var searchListResponse = searchListRequest.Execute();

                        List<string> _search = new List<string>();

                        int currentresult = 0;
                        foreach (var searchResult in searchListResponse.Items)
                        {
                            if (currentresult != 5)
                            {
                                _search.Add($"**{searchResult.Title}**\n{searchResult.Snippet}\n{searchResult.Link}\n");
                                currentresult += 1;
                            }
                        }

                        string response = string.Format(string.Join("\n", _search));

                        EmbedBuilder embed = new EmbedBuilder();
                        EmbedFooterBuilder embedfoot = new EmbedFooterBuilder();
                        embed.Title = $"Google Search For '{search}'";
                        embed.WithDescription($"{response}");

                        embedfoot.WithIconUrl(Context.User.GetAvatarUrl());
                        embedfoot.WithText("Commanded issued by " + Context.User);

                        embed.WithFooter(embedfoot);                    
                        embed.WithColor(googleColor);

                        return embed;
                        
                    }
                    catch (Exception ex)
                    {
                        Global.ColorMessage($"[{Global.TimeNow()}] An error occured while user '{Context.User}' tryied searching '{search}' on google. \nError Details: \n{ex.Message}", ConsoleColor.Red);

                        EmbedBuilder embed = new EmbedBuilder
                        {
                            Title = "Google Search Error"
                        };
                        embed.WithDescription($"An Error Occured. It is best to tell the owner of this bot this error.\n**Error Details: ** {ex.Message}");
                        embed.WithColor(googleColor);

                        return embed;
                    }
                }
                else
                {
                    EmbedBuilder embed = new EmbedBuilder
                    {
                        Title = "Google Search Error"
                    };
                    embed.WithDescription($"We are sorry, but google search is disabled by the bot owner.");
                    embed.WithColor(googleColor);

                    return embed;
                }
            }
            else
            {
                EmbedBuilder embed = new EmbedBuilder
                {
                    Title = "Google Search Error"
                };
                embed.WithDescription($"Don't you want to search something?\nE.G: {Config.bot.botPrefix}google Dank Memes");
                embed.WithColor(new Color(53, 169, 84));

                return embed;
            }
        }
    }
}
