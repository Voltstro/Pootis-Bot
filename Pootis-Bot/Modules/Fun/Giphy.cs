using Discord;
using Discord.Commands;
using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Pootis_Bot.Modules.Fun
{
    public class Giphy : ModuleBase<SocketCommandContext>
    {
        [Command("giphy")]
        [Alias("gy")]
        public async Task GiphySearch([Remainder] string search = "")
        {
            if(search != "")
            {
                string input = search.Replace(" ", "+");

                string json = "";
                using (WebClient client = new WebClient())
                {
                    json = client.DownloadString($"http://api.giphy.com/v1/gifs/search?q={input}&api_key=" + Config.bot.apiGiphyKey);
                }

                var dataObject = JsonConvert.DeserializeObject<dynamic>(json);

                int choose = Wording.RandomNumber(0, 25);

                string url = dataObject.data[choose].images.fixed_height.url.ToString();
                string title = dataObject.data[choose].title.ToString();
                string author = dataObject.data[choose].username.ToString();
                string shorturl = dataObject.data[choose].bitly_gif_url.ToString();

                EmbedBuilder embed = new EmbedBuilder();
                EmbedFooterBuilder embedfoot = new EmbedFooterBuilder();
                embed.Title = Wording.Title(title);
                embed.WithImageUrl(url);

                embedfoot.WithIconUrl(Context.User.GetAvatarUrl());
                embedfoot.WithText("Commanded issued by " + Context.User);

                embed.WithFooter(embedfoot);
                embed.WithDescription($"BY: {author}\nURL: {shorturl}");
                embed.WithColor(new Color(190, 101, 249));
                await Context.Channel.SendMessageAsync("", false, embed);
            }
            else
            {
                EmbedBuilder embed = new EmbedBuilder
                {
                    Title = "Search Error"
                };
                embed.WithDescription($"Don't you want to search something?\nE.G: {Config.bot.botPrefix}giphy Funny Cats");
                embed.WithColor(new Color(190, 101, 249));
                await Context.Channel.SendMessageAsync("", false, embed);
            }          
        } 
    }
}
