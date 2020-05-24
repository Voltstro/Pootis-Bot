using System;
using Pootis_Bot.Core;
using Steam.Models.SteamCommunity;
using SteamWebAPI2.Exceptions;
using SteamWebAPI2.Interfaces;
using SteamWebAPI2.Utilities;

namespace Pootis_Bot.Services
{
	public class SteamService
	{
		public static bool IsEnabled;

		private static SteamUser steamUserInterface;
		private static PlayerService steamPlayerInterface;

		/// <summary>
		/// Sets up the <see cref="SteamService"/>
		/// </summary>
		public static void SetupSteam()
		{
			if (IsEnabled) return;

			if (!string.IsNullOrWhiteSpace(Config.bot.Apis.ApiSteamKey))
			{
				SteamWebInterfaceFactory webInterface = new SteamWebInterfaceFactory(Config.bot.Apis.ApiSteamKey);
				steamUserInterface = webInterface.CreateSteamWebInterface<SteamUser>(Global.HttpClient);
				steamPlayerInterface = webInterface.CreateSteamWebInterface<PlayerService>(Global.HttpClient);

				IsEnabled = true;
			}
			else
			{
				throw new Exception("The config doesn't have the Steam API key set!");
			}
		}

		#region Steam User Methods

		/// <summary>
		/// Gets a Steam ID from a custom vanity URL, CANNOT include the https://steamcommunity.com part
		/// </summary>
		/// <param name="user"></param>
		/// <returns>Returns the user ID, if found</returns>
		public static ulong GetSteamIdFromCustomUrl(string user)
		{
			try
			{
				ulong id = steamUserInterface.ResolveVanityUrlAsync(user).GetAwaiter().GetResult().Data;
				return id;
			}
			catch (VanityUrlNotResolvedException)
			{
				return 0;
			}
		}

		/// <summary>
		/// Gets a Steam user's <see cref="PlayerSummaryModel"/>
		/// </summary>
		/// <param name="steamId"></param>
		/// <returns></returns>
		public static PlayerSummaryModel GetSteamUserSummary(ulong steamId)
		{
			return steamUserInterface.GetPlayerSummaryAsync(steamId).GetAwaiter().GetResult().Data;
		}

		#endregion

		#region Steam Player Methods

		/// <summary>
		/// Gets a Steam user's Steam level
		/// </summary>
		/// <param name="steamId"></param>
		/// <returns></returns>
		public static uint GetSteamUserLevel(ulong steamId)
		{
			uint? level = steamPlayerInterface.GetSteamLevelAsync(steamId).GetAwaiter().GetResult().Data;

			return level ?? 0;
		}

		/// <summary>
		/// Gets a Steam user's games
		/// </summary>
		/// <param name="steamId"></param>
		/// <returns></returns>
		public static OwnedGamesResultModel GetSteamUserGames(ulong steamId)
		{
			return steamPlayerInterface.GetOwnedGamesAsync(steamId, true, true).GetAwaiter().GetResult().Data;
		}

		#endregion
	}
}