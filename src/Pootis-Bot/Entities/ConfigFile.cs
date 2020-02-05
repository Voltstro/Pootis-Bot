using Newtonsoft.Json;
using Pootis_Bot.Structs.Config;

namespace Pootis_Bot.Entities
{
	public class ConfigFile
	{
		/// <summary>
		/// The config version
		/// </summary>
		public string ConfigVersion { get; set; }

		/// <summary>
		/// The bot's name
		/// </summary>
		public string BotName { get; set; }

		/// <summary>
		/// The bot's prefix
		/// </summary>
		public string BotPrefix { get; set; }

		/// <summary>
		/// The bot's token
		/// </summary>
		public string BotToken { get; set; }

		/// <summary>
		/// Who's stream should the bot show when set in streaming mode
		/// </summary>
		public string TwitchStreamingSite { get; set; }

		/// <summary>
		/// Should we check to see if we are still connected periodically
		/// </summary>
		public bool CheckConnectionStatus { get; set; }

		/// <summary>
		/// How ofter should we check if we are still connected (milliseconds)
		/// </summary>
		public int CheckConnectionStatusInterval { get; set; }

		/// <summary>
		/// The default game status the bot should use
		/// </summary>
		public string DefaultGameMessage { get; set; }

		/// <summary>
		/// How much xp to give?
		/// </summary>
		public uint LevelUpAmount { get; set; }

		/// <summary>
		/// The time between leveling up
		/// </summary>
		public int LevelUpCooldown { get; set; }

		/// <summary>
		/// Should we put debug messages into the console and log file
		/// </summary>
		public bool LogDebugMessages { get; set; }

		/// <summary>
		/// Reports errors to the bot owner
		/// </summary>
		public bool ReportErrorsToOwner { get; set; }

		/// <summary>
		/// Reports events such as when the bot joins/leaves a guild
		/// </summary>
		public bool ReportGuildEventsToOwner { get; set; }

		/// <summary>
		/// What type of formatting should we use for ServerList.json and UserAccounts.json
		/// </summary>
		public Formatting ResourceFilesFormatting { get; set; }

		/// <summary>
		/// API settings
		/// </summary>
		[JsonProperty("ApiKeys")] public ConfigApis Apis;

		/// <summary>
		/// Audio settings
		/// </summary>
		public ConfigAudio AudioSettings;

		/// <summary>
		/// Setting related to voting
		/// </summary>
		public VoteSettings VoteSettings;
	}
}