using System;
using System.Globalization;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Steam.Models.SteamCommunity;
using Pootis_Bot.Helpers;
using Pootis_Bot.Services;

namespace Pootis_Bot.Modules.Steam
{
	public class SteamUserUtils : ModuleBase<SocketCommandContext>
	{
		[Command("steam search")]
		[Summary("Search steam for a user")]
		public async Task SteamSearch(string user)
		{
			if (SteamService.IsEnabled)
			{
				EmbedBuilder embed = new EmbedBuilder();
				embed.WithTitle($"Steam User Search '{user}'");
				embed.WithDescription("Searching Steam...");
				embed.WithColor(51, 66, 89);

				IUserMessage message = await Context.Channel.SendMessageAsync("", false, embed.Build());

				ulong.TryParse(user, NumberStyles.None, CultureInfo.InvariantCulture, out ulong id);

				//If the id is 0, then try and get the steam profile using a vanity url search
				if (id == 0)
				{
					id = SteamService.GetSteamIdFromCustomUrl(user);
				}

				//If the Id is still 0, then their is no Steam profile found
				if (id == 0)
				{
					embed.WithDescription("No Steam profile could be found!");
				}
				else
				{
					PlayerSummaryModel userSummary = SteamService.GetSteamUserSummary(id);

					string countryDetails = userSummary.CountryCode != null ? $":flag_{userSummary.CountryCode.ToLower()}: {userSummary.CountryCode}" : "No Country Provided";

					embed.WithDescription("");
					embed.AddField("User Profile", $"**Username**: {userSummary.Nickname}\n" +
					                               $"**Status**: {userSummary.UserStatus}\n" +
					                               $"**Country**: {countryDetails}", true);

					embed.AddField("Account Details", $"**Creation Date**: {userSummary.AccountCreatedDate}\n" +
					                                  $"**Last Logged Off Date**: {userSummary.LastLoggedOffDate}\n" +
					                                  $"**Steam ID**: {userSummary.SteamId}", true);

					embed.WithThumbnailUrl(userSummary.AvatarFullUrl);
				}

				await MessageUtils.ModifyMessage(message, embed);
			}
			else
			{
				await Context.Channel.SendMessageAsync("The Steam services are disabled!");
			}
		}
	}
}
