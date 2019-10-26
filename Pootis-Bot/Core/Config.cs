using System;
using System.IO;
using Newtonsoft.Json;
using Pootis_Bot.Entities;
using Pootis_Bot.Structs;

namespace Pootis_Bot.Core
{
	/// <summary>
	/// Stores config for the bot!
	/// </summary>
	public static class Config
	{
		private const string ConfigFolder = "Resources";
		private const string ConfigFile = "Config.json";

		private const string ConfigVersion = "7";

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

				Global.Log("Config.json was created. Is this your first time running?", ConsoleColor.Yellow);
			}
			else
			{
				string json =
					File.ReadAllText(ConfigFolder + "/" + ConfigFile); //If it does exist then it continues like normal.
				bot = JsonConvert.DeserializeObject<ConfigFile>(json);

				if (string.IsNullOrWhiteSpace(bot.ConfigVersion) || (bot.ConfigVersion != ConfigVersion))
				{
					bot.ConfigVersion = ConfigVersion;
					SaveConfig();
					Global.Log("Updated config to version " + ConfigVersion, ConsoleColor.Yellow);
				}
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
				LevelUpCooldown = 15,
				LevelUpAmount = 10,
				AudioSettings = new ConfigAudio
				{
					AudioServicesEnabled = false,
					InitialApplication = "External\\youtube-dl",
					PythonArguments = ""
				},
				CheckConnectionStatus = true,
				CheckConnectionStatusInterval = 60000,
				DefaultGameMessage = "Use $help for help.",
				Apis = new ConfigApis()
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