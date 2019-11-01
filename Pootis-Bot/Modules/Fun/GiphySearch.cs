using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.Rest;
using Pootis_Bot.Core;
using Pootis_Bot.Preconditions;
using Pootis_Bot.Services.Fun;
using Pootis_Bot.Structs;

namespace Pootis_Bot.Modules.Fun
{
	public class GiphySearch : ModuleBase<SocketCommandContext>
	{
		// Module Information
		// Original Author  - Creepysin
		// Description      - Searches Giphy
		// Contributors     - Creepysin, 

		[Command("giphy")]
		[Summary("Searches Giphy")]
		[Alias("gy")]
		[Cooldown(5)]
		[RequireBotPermission(GuildPermission.EmbedLinks)]
		[RequireBotPermission(GuildPermission.AttachFiles)]
		public async Task Giphy([Remainder] string search = "")
		{
			if (string.IsNullOrWhiteSpace(Config.bot.Apis.ApiGiphyKey))
			{
				await Context.Channel.SendMessageAsync("Giphy search is disabled by the bot owner.");
				return;
			}

			if (string.IsNullOrWhiteSpace(search))
			{
				await Context.Channel.SendMessageAsync("The search input cannot be blank!");
				return;
			}

			EmbedBuilder embed = new EmbedBuilder();
			embed.WithTitle($"Giphy Search '{search}'");
			embed.WithDescription("Searching Giphy...");
			embed.WithFooter($"Search by {Context.User}", Context.User.GetAvatarUrl());
			embed.WithCurrentTimestamp();
			embed.WithColor(FunCmdsConfig.giphyColor);

			RestUserMessage message = await Context.Channel.SendMessageAsync("", false, embed.Build());

			GiphySearchResult results = GiphyService.Search(search);
			if (!results.IsSuccessful)
			{
				if (results.ErrorReason == ErrorReason.Error)
				{
					await Context.Channel.SendMessageAsync(
						"Sorry, but an error occured while searching Giphy, please try again in a moment!");
					return;
				}
			}

			embed.WithDescription($"**By**: {results.Data.GifAuthor}\n**URL**: {results.Data.GifLink}");
			embed.WithImageUrl(results.Data.GifUrl);
			embed.WithCurrentTimestamp();

			await message.ModifyAsync(x => { x.Embed = embed.Build(); });
		}
	}
}