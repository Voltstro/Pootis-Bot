using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Pootis_Bot.Core;
using Pootis_Bot.Entities;
using Pootis_Bot.Services.Fun;

namespace Pootis_Bot.Modules.Fun
{
    public class GiphySearch : ModuleBase<SocketCommandContext>
    {
        // Module Infomation
        // Orginal Author   - Creepysin
        // Description      - Searches Giphy
        // Contributors     - Creepysin, 

        [Command("giphy")]
        [Summary("Searches Giphy")]
        [Alias("gy")]
        [RequireBotPermission(GuildPermission.EmbedLinks)]
        [RequireBotPermission(GuildPermission.AttachFiles)]
        public async Task CmdGiphySearch([Remainder] string search = "")
        {
            if (string.IsNullOrWhiteSpace(Config.bot.apis.apiGiphyKey))
            {
                await Context.Channel.SendMessageAsync("Giphy search is disabled by the bot owner.");
                return;
            }

            GiphyData results = GiphyService.Search(search);

            EmbedBuilder embed = new EmbedBuilder();
            embed.WithTitle("Giphy Search: " + Global.Title(results.gifTitle));
            embed.WithDescription($"**By**: {results.gifAuthor}\n**URL**: {results.GifLink}");
            embed.WithImageUrl(results.gifUrl);
            embed.WithFooter($"Search by {Context.User} @ ", Context.User.GetAvatarUrl());
            embed.WithCurrentTimestamp();
            embed.WithColor(FunCmdsConfig.giphyColor);

            await Context.Channel.SendMessageAsync("", false, embed.Build());
        }
    }
}
