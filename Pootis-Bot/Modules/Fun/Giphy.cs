using System;
using System.Net;
using System.Threading.Tasks;
using System.Linq;
using Newtonsoft.Json;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Pootis_Bot.Core;

namespace Pootis_Bot.Modules.Fun
{
    public class Giphy : ModuleBase<SocketCommandContext>
    {
        readonly Color giphyColor = new Color(190, 101, 249);

        [Command("giphy")]
        [Alias("gy")]
        public async Task CmdGiphySearch([Remainder] string search = "")
        {          
            var server = ServerLists.GetServer(Context.Guild);

            //Check to see if the command has a permission set
            if (server.permissions.PermGiphy != null && server.permissions.PermGiphy != "")
            {
                var _user = Context.User as SocketGuildUser;
                var setrole = (_user as IGuildUser).Guild.Roles.FirstOrDefault(x => x.Name == server.permissions.PermGiphy);

                if(_user.Roles.Contains(setrole))
                    await Context.Channel.SendMessageAsync("", false, GiphySearch(search).Build());                         
            }
            else
                await Context.Channel.SendMessageAsync("", false, GiphySearch(search).Build());   
        } 

        EmbedBuilder GiphySearch(string search)
        {
            if (search.Trim() != "") //Check to see if search is nothing
            {
                if (Config.bot.apis.apiGiphyKey.Trim() != "") //Check to see if the bot giphy api is nothing
                {
                    try
                    {
                        string input = search.Replace(" ", "+");

                        string json = "";
                        using (WebClient client = new WebClient()) //Search the term using the giphy api; More about the api here: https://developers.giphy.com/docs/
                        {
                            json = client.DownloadString($"http://api.giphy.com/v1/gifs/search?q={input}&api_key=" + Config.bot.apis.apiGiphyKey);
                        }

                        var dataObject = JsonConvert.DeserializeObject<dynamic>(json);

                        int choose = Global.RandomNumber(0, 25);

                        //Read the json file
                        string url = dataObject.data[choose].images.fixed_height.url.ToString();
                        string title = dataObject.data[choose].title.ToString();
                        string author = dataObject.data[choose].username.ToString();
                        string shorturl = dataObject.data[choose].bitly_gif_url.ToString();

                        //Build the embed and return it.
                        EmbedBuilder embed = new EmbedBuilder();
                        EmbedFooterBuilder embedfoot = new EmbedFooterBuilder();
                        embed.Title = Global.Title(title);
                        embed.WithImageUrl(url);

                        embedfoot.WithIconUrl(Context.User.GetAvatarUrl());
                        embedfoot.WithText("Commanded issued by " + Context.User);

                        embed.WithFooter(embedfoot);
                        embed.WithDescription($"BY: {author}\nURL: {shorturl}");
                        embed.WithColor(giphyColor);

                        return embed;

                    }
                    catch (Exception ex)
                    {
                        Global.WriteMessage($"[{Global.TimeNow()}] An error occured while user '{Context.User}' tryied searching '{search}' on giphy. \nError Details: \n{ex.Message}", ConsoleColor.Red);

                        EmbedBuilder embed = new EmbedBuilder
                        {
                            Title = "Giphy Search Error"
                        };
                        embed.WithDescription($"An Error Occured. It is best to tell the owner of this bot this error.\n**Error Details: ** {ex.Message}");
                        embed.WithColor(giphyColor);

                        return embed;
                    }
                }
                else
                {
                    EmbedBuilder embed = new EmbedBuilder
                    {
                        Title = "Giphy Search Error"
                    };
                    embed.WithDescription($"We are sorry, but giphy search is disabled by the bot owner.");
                    embed.WithColor(giphyColor);

                    return embed;
                }
            }
            else
            {
                EmbedBuilder embed = new EmbedBuilder
                {
                    Title = "Giphy Search Error"
                };
                embed.WithDescription($"Don't you want to search something?\nE.G: {Config.bot.botPrefix}giphy Funny Cats");
                embed.WithColor(giphyColor);

                return embed;
            }   
        }
    }
}
