using System;
using System.IO;
using Newtonsoft.Json;
using Pootis_Bot.Core.Logging;
using Pootis_Bot.Entities;
using Pootis_Bot.Structs.Config;

namespace Pootis_Bot.Core
{
	/// <summary>
	/// Manages the bot's config
	/// </summary>
	public static class Config
	{
		private const string ConfigFolder = "Resources";
		private const string ConfigFile = "Config.json";

		private const string ConfigVersion = "13";

		public static readonly ConfigFile bot;

		static Config()
		{
			if (!Directory.Exists(ConfigFolder)) //Creates the Resources folder if it doesn't exist.
				Directory.CreateDirectory(ConfigFolder);

			//If the config.json file doesn't exist it create a new one.
			if (!File.Exists(ConfigFolder + "/" + ConfigFile))
			{
				bot = NewConfig();

				SaveConfig();

				Logger.Log("Config.json was created. Is this your first time running?", LogVerbosity.Warn);
			}
			else
			{
				string json =
					File.ReadAllText(ConfigFolder + "/" + ConfigFile); //If it does exist then it continues like normal.
				bot = JsonConvert.DeserializeObject<ConfigFile>(json);

				if (!string.IsNullOrWhiteSpace(bot.ConfigVersion) && bot.ConfigVersion == ConfigVersion) return;

				bot.ConfigVersion = ConfigVersion;
				SaveConfig();
				Logger.Log("Updated config to version " + ConfigVersion, LogVerbosity.Warn);
			}
		}

		public static ConfigFile NewConfig()
		{
			ConfigFile newConfig = new ConfigFile
			{
				ConfigVersion = ConfigVersion,
				BotName = "CSharp Bot",
				BotPrefix = "$",
				BotToken = "",
				ResourceFilesFormatting = Formatting.Indented,
				ReportErrorsToOwner = false,
				ReportGuildEventsToOwner = false,
				TwitchStreamingSite = "https://www.twitch.tv/creepysin",
				LevelUpCooldownTime = new TimeSpan(0, 0, 15),
				LevelUpCooldown = 0,
				CheckConnectionStatus = true,
				CheckConnectionStatusInterval = 60000,
				DefaultGameMessage = "Use $help for help.",
				Apis = new ConfigApis(),
				AudioSettings = new ConfigAudio
				{
					AudioServicesEnabled = false,
					LogPlayStopSongToConsole = true,
					MaxVideoTime = new TimeSpan(0, 7, 0)
				},
				VoteSettings = new VoteSettings
				{
					MaxRunningVotesPerGuild = 3,
					MaxVoteTime = new TimeSpan(7, 0, 0, 0)
				}
			};

			return newConfig;
		}

		/// <summary>
		/// Saves the config, DUH!
		/// </summary>
		public static void SaveConfig()
		{
			string json = JsonConvert.SerializeObject(bot, Formatting.Indented);
			File.WriteAllText(ConfigFolder + "/" + ConfigFile, json);
		}
	}
}