using Discord;
using Discord.Commands;
using Google.Apis.Customsearch.v1;
using Google.Apis.Services;
using Pootis_Bot.Core;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pootis_Bot.Modules.Fun
{
    public class Google : ModuleBase<SocketCommandContext>
    {
        [Command("google")]
        [Alias("g")]
        public async Task GoogleSearch([Remainder]string search = "")
        {
            if (search != "")
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

                    var searchListResponse = await searchListRequest.ExecuteAsync();

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
                    embed.WithColor(new Color(53, 169, 84));
                    await Context.Channel.SendMessageAsync("", false, embed.Build());
                }
                catch(Exception ex)
                {
                    Global.ColorMessage($"[{Global.TimeNow()}] An error occured while user '{Context.User}' tryied searching '{search}' on google. \nError Details: \n{ex.Message}", ConsoleColor.Red);

                    EmbedBuilder embed = new EmbedBuilder
                    {
                        Title = "Google Search Error"
                    };
                    embed.WithDescription($"An Error Occured. It is best to tell the owner of this bot this error.\n**Error Details: ** {ex.Message}");
                    embed.WithColor(new Color(53, 169, 84));
                    await Context.Channel.SendMessageAsync("", false, embed.Build());
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
                await Context.Channel.SendMessageAsync("", false, embed.Build());
            }
        }
    }
}
