using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Pootis_Bot.Core;
using Pootis_Bot.Entities;
using Pootis_Bot.Structs;
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

            var results = GiphyService.Search(search);
			if (!results.IsSuccessfull)
			{
				//This should never happen, since we check it at the start!
				if(results.ErrorReason == ErrorReason.NoAPIKey)
					return;
				if(results.ErrorReason == ErrorReason.Error)
				{
					await Context.Channel.SendMessageAsync("Sorry, but an error occured while searching Giphy, please try again in a moment!");
					return;
				}
			}

            EmbedBuilder embed = new EmbedBuilder();
            embed.WithTitle("Giphy Search: " + Global.Title(results.Data.gifTitle));
            embed.WithDescription($"**By**: {results.Data.gifAuthor}\n**URL**: {results.Data.GifLink}");
            embed.WithImageUrl(results.Data.gifUrl);
            embed.WithFooter($"Search by {Context.User} @ ", Context.User.GetAvatarUrl());
            embed.WithCurrentTimestamp();
            embed.WithColor(FunCmdsConfig.giphyColor);

            await Context.Channel.SendMessageAsync("", false, embed.Build());
        }
    }
}
