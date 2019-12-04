using Newtonsoft.Json;
using Pootis_Bot.Structs.Config;

namespace Pootis_Bot.Entities
{
	public class ConfigFile
	{
		/// <summary>
		///     Api settings
		/// </summary>
		[JsonProperty("ApiKeys")] public ConfigApis Apis;

		/// <summary>
		///     Audio settings
		/// </summary>
		public ConfigAudio AudioSettings;

		/// <summary>
		///     The bot's name
		/// </summary>
		public string BotName;

		/// <summary>
		///     The bot's prefix
		/// </summary>
		public string BotPrefix;

		/// <summary>
		///     The bot's token
		/// </summary>
		public string BotToken;

		/// <summary>
		///     Whether or not we should check to see if we are still connected
		/// </summary>
		public bool CheckConnectionStatus;

		/// <summary>
		///     How ofter should we check if we are still connected (milliseconds)
		/// </summary>
		public int CheckConnectionStatusInterval;

		/// <summary>
		///     The config version
		/// </summary>
		public string ConfigVersion;

		/// <summary>
		///     The default game status the bot should use
		/// </summary>
		public string DefaultGameMessage;

		/// <summary>
		///     How much xp to give?
		/// </summary>
		public uint LevelUpAmount;

		/// <summary>
		///     How long between each message should we wait before allowing to give more XP
		/// </summary>
		public int LevelUpCooldown;

		/// <summary>
		///     Reports errors to the bot owner
		/// </summary>
		public bool ReportErrorsToOwner;

		/// <summary>
		///     Reports events such as when the bot joins/leaves a guild
		/// </summary>
		public bool ReportGuildEventsToOwner;

		/// <summary>
		///     What type of formatting we should use for ServerList.json and UserAccounts.json
		/// </summary>
		public Formatting ResourceFilesFormatting;

		/// <summary>
		///     Who's stream should the bot show when set in streaming mode
		/// </summary>
		public string TwitchStreamingSite;
	}
}