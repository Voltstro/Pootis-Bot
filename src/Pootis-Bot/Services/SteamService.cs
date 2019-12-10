using System;
using System.Collections.Generic;
using SteamWebAPI2.Interfaces;
using SteamWebAPI2.Utilities;
using Steam.Models.SteamCommunity;
using Pootis_Bot.Core;
using SteamWebAPI2.Exceptions;

namespace Pootis_Bot.Services
{
	public class SteamService
	{
		public static bool IsEnabled;

		private static SteamUser _steamUserInterface;
		private static PlayerService _steamPlayerInterface;

		public static void SetupSteam()
		{
			if (IsEnabled) return;

			if (!string.IsNullOrWhiteSpace(Config.bot.Apis.ApiSteamKey))
			{
				SteamWebInterfaceFactory webInterface = new SteamWebInterfaceFactory(Config.bot.Apis.ApiSteamKey);
				_steamUserInterface = webInterface.CreateSteamWebInterface<SteamUser>(Global.HttpClient);
				_steamPlayerInterface = webInterface.CreateSteamWebInterface<PlayerService>(Global.HttpClient);

				IsEnabled = true;
			}
			else
			{
				throw new Exception("The config doesn't have to Steam API key set!");
			}
		}

		#region Steam User Methods

		public static ulong GetSteamIdFromCustomUrl(string user)
		{
			try
			{
				ulong id = _steamUserInterface.ResolveVanityUrlAsync(user).GetAwaiter().GetResult().Data;
				return id;
			}
			catch (VanityUrlNotResolvedException)
			{
				return 0;
			}
		}

		public static PlayerSummaryModel GetSteamUserSummary(ulong steamId)
		{
			return _steamUserInterface.GetPlayerSummaryAsync(steamId).GetAwaiter().GetResult().Data;
		}

		#endregion

		#region Steam Player Methods

		public static uint GetSteamUserLevel(ulong steamId)
		{
			uint? level = _steamPlayerInterface.GetSteamLevelAsync(steamId).GetAwaiter().GetResult().Data;

			return level ?? 0;
		}

		public static OwnedGamesResultModel GetSteamUserGames(ulong steamId)
		{
			return _steamPlayerInterface.GetOwnedGamesAsync(steamId, true, true).GetAwaiter().GetResult().Data;
		}

		#endregion
	}
}
