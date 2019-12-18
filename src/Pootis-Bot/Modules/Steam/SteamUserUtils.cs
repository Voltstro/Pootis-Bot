using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Pootis_Bot.Helpers;
using Pootis_Bot.Services;
using Steam.Models.SteamCommunity;

namespace Pootis_Bot.Modules.Steam
{
	public class SteamUserUtils : ModuleBase<SocketCommandContext>
	{
		[Command("steam search")]
		[Summary("Searches Steam for a user (either an id or a vanity url)")]
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
					id = SteamService.GetSteamIdFromCustomUrl(user.StartsWith("https://steamcommunity.com/id/")
						? user.Replace("https://steamcommunity.com/id/", "").ToLower()
						: user.ToLower());

				//If the Id is still 0, then their is no Steam profile found
				if (id == 0)
				{
					embed.WithDescription("No Steam profile could be found!");
				}
				else
				{
					PlayerSummaryModel userSummary = SteamService.GetSteamUserSummary(id);

					string countryDetails = userSummary.CountryCode != null
						? $":flag_{userSummary.CountryCode.ToLower()}: {userSummary.CountryCode}"
						: "No Country Provided";

					embed.WithDescription(
						$"[Backpack.tf Profile](https://backpack.tf/u/{userSummary.SteamId}) - [Rep.tf Summary](https://rep.tf/{userSummary.SteamId}) - [Steam Profile](https://steamcommunity.com/profiles/{userSummary.SteamId})");
					embed.AddField("User Profile", $"**Username**: {userSummary.Nickname}\n" +
					                               $"**Status**: {userSummary.UserStatus}\n" +
					                               $"**Country**: {countryDetails}\n" +
					                               $"**Level**: {SteamService.GetSteamUserLevel(id)}", true);

					string accountDetails = $"**Last Logged Off Date**: {userSummary.LastLoggedOffDate}\n";

					if (userSummary.ProfileVisibility == ProfileVisibility.Private ||
					    userSummary.ProfileVisibility == ProfileVisibility.Unknown ||
					    userSummary.ProfileVisibility == ProfileVisibility.FriendsOnly)
						accountDetails += "**Creation Date**: Unknown\n";
					else
						accountDetails += $"**Creation Date**: {userSummary.AccountCreatedDate}\n" +
						                  $"**Years Served**: {DateTime.UtcNow.Year - userSummary.AccountCreatedDate.Year}\n";

					accountDetails += $"**Profile Privacy**: {userSummary.ProfileVisibility}\n" +
					                  $"**Steam ID**: {userSummary.SteamId}";

					embed.AddField("Account Details", accountDetails, true);

					//Games
					OwnedGamesResultModel games = SteamService.GetSteamUserGames(id);
					if (games?.OwnedGames != null)
					{
						//Get all user games and sort them by most played
						List<OwnedGameModel> sortedGames =
							games.OwnedGames.ToList().OrderByDescending(x => x.PlaytimeForever).ToList();

						//Make sure the user has a game on their account
						if (sortedGames.Count != 0)
						{
							TimeSpan totalTfHours = TimeSpan.Zero;
							TimeSpan totalCsHours = TimeSpan.Zero;
							TimeSpan totalDotaHours = TimeSpan.Zero;

							TimeSpan totalAmountOfHours = TimeSpan.Zero;
							foreach (OwnedGameModel game in sortedGames)
							{
								totalAmountOfHours += game.PlaytimeForever;

								//Get hours if it is a supported game
								// ReSharper disable once ConvertIfStatementToSwitchStatement
								if (game.AppId == 440) //Team Fortress 2
									totalTfHours = game.PlaytimeForever;
								else if (game.AppId == 730) //Counter-Strike: Global Offensive (CS:GO)
									totalCsHours = game.PlaytimeForever;
								else if (game.AppId == 570) //Dota 2
									totalDotaHours = game.PlaytimeForever;
							}

							string gameStatus;

							// ReSharper disable once CompareOfFloatsByEqualityOperator
							if (Math.Round(totalAmountOfHours.TotalHours) == 0)
							{
								if (games.GameCount == 0 && userSummary.ProfileVisibility == ProfileVisibility.Private)
									gameStatus = "Private profile...";
								else if (games.GameCount != 0)
									gameStatus = "Looks like this person hides their game hours...";
								else
									gameStatus = "This person has never played a game on Steam... wow...";
							}
							else
							{
								gameStatus = $"**Total Hours**: {Math.Round(totalAmountOfHours.TotalHours)}\n" +
								             $"**Hours in most played Game**: {Math.Round(sortedGames[0].PlaytimeForever.TotalHours)} ({sortedGames[0].Name})";
							}

							embed.AddField("Games", $"**Games Amount**: {games.GameCount}\n{gameStatus}");
							embed.AddField("Game Specific Hours",
								$"**Team Fortress 2**: {Math.Round(totalTfHours.TotalHours)}hrs\n" +
								$"**Counter-Strike: Global Offensive (CS:GO)**: {Math.Round(totalCsHours.TotalHours)}hrs\n" +
								$"**Dota 2**: {Math.Round(totalDotaHours.TotalHours)}hrs", true);
						}
					}
					else
					{
						embed.AddField("Games", "User games are hidden...");
					}

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