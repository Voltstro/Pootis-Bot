using System.Collections.Generic;
using Pootis_Bot.Structs;

namespace Pootis_Bot.Entities
{
	public class GlobalConfigFile
	{
		//Bot APIs
		public ConfigApis Apis;
		public string BotName = "Bot";
		public string BotPrefix = "$";

		public string BotToken = "";

		public bool CheckConnectionStatus = true;
		public int CheckConnectionStatusInterval = 60000;
		public string ConfigVersion;
		public string GameMessage = "Use $help for help.";

		//Help Modules
		public List<HelpModule> HelpModules = new List<HelpModule>();
		public bool IsAudioServiceEnabled = false;

		public int LevelUpCooldown = 15;
		public string TwitchStreamingSite = "https://www.twitch.tv/creepysin";

		/// <summary>
		/// Creates a new HelpModule class (Wow!)
		/// </summary>
		public class HelpModule
		{
			public string Group;
			public List<string> Modules = new List<string>();
		}
	}
}