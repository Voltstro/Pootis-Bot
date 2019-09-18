using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Pootis_Bot.Core;
using Pootis_Bot.Structs;
using Pootis_Bot.Services.Fun;

namespace Pootis_Bot.Modules.Fun
{
    public class GiphySearch : ModuleBase<SocketCommandContext>
    {
        // Module Information
        // Original Author   - Creepysin
        // Description      - Searches Giphy
        // Contributors     - Creepysin, 

        [Command("giphy")]
        [Summary("Searches Giphy")]
        [Alias("gy")]
        [RequireBotPermission(GuildPermission.EmbedLinks)]
        [RequireBotPermission(GuildPermission.AttachFiles)]
        public async Task CmdGiphySearch([Remainder] string search = "")
        {
            if (string.IsNullOrWhiteSpace(Config.bot.Apis.ApiGiphyKey))
            {
                await Context.Channel.SendMessageAsync("Giphy search is disabled by the bot owner.");
                return;
            }

            var results = GiphyService.Search(search);
			if (!results.IsSuccessful)
			{
				//This should never happen, since we check it at the start!
				if(results.ErrorReason == ErrorReason.NoApiKey)
					return;
				if(results.ErrorReason == ErrorReason.Error)
				{
					await Context.Channel.SendMessageAsync("Sorry, but an error occured while searching Giphy, please try again in a moment!");
					return;
				}
			}

            EmbedBuilder embed = new EmbedBuilder();
            embed.WithTitle("Giphy Search: " + Global.Title(results.Data.GifTitle));
            embed.WithDescription($"**By**: {results.Data.GifAuthor}\n**URL**: {results.Data.GifLink}");
            embed.WithImageUrl(results.Data.GifUrl);
            embed.WithFooter($"Search by {Context.User} @ ", Context.User.GetAvatarUrl());
            embed.WithCurrentTimestamp();
            embed.WithColor(FunCmdsConfig.giphyColor);

            await Context.Channel.SendMessageAsync("", false, embed.Build());
        }
    }
}
