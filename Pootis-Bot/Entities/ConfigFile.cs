using System.Collections.Generic;
using Pootis_Bot.Structs;

namespace Pootis_Bot.Entities
{
	public class ConfigFile
	{
		/// <summary>
		/// The config version
		/// </summary>
		public string ConfigVersion;

		/// <summary>
		/// The bot's name
		/// </summary>
		public string BotName;

		/// <summary>
		/// The bot's prefix
		/// </summary>
		public string BotPrefix;

		/// <summary>
		/// The bot's token
		/// </summary>
		public string BotToken;

		/// <summary>
		/// Reports errors to the bot owner
		/// </summary>
		public bool ReportErrorsToOwner;

		/// <summary>
		/// Reports events such as when the bot joins/leaves a guild
		/// </summary>
		public bool ReportGuildEventsToOwner;

		/// <summary>
		/// Who's stream should the bot show when set in streaming mode
		/// </summary>
		public string TwitchStreamingSite;

		/// <summary>
		/// How long between each message should we wait before allowing to give more XP
		/// </summary>
		public int LevelUpCooldown;

		/// <summary>
		/// Are the audio services enabled?
		/// </summary>
		public bool IsAudioServiceEnabled;

		/// <summary>
		/// Whether or not we should check to see if we are still connected
		/// </summary>
		public bool CheckConnectionStatus;

		/// <summary>
		/// How ofter should we check if we are still connected (milliseconds)
		/// </summary>
		public int CheckConnectionStatusInterval;

		/// <summary>
		/// The default game status the bot should use
		/// </summary>
		public string DefaultGameMessage;

		/// <summary>
		/// Api settings
		/// </summary>
		public ConfigApis Apis;

		/// <summary>
		/// The help modules, basically what commands do you want in your help message
		/// </summary>
		public List<HelpModule> HelpModules;

		public class HelpModule
		{
			public string Group;
			public List<string> Modules;
		}
	}
}