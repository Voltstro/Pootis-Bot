using System.Collections.Generic;
using Pootis_Bot.Structs;

namespace Pootis_Bot.Entities
{
    public class GlobalConfigFile
    {
        public string ConfigVersion;

        public string BotToken = "";
        public string BotPrefix = "$";
        public string BotName = "Bot";
        public string TwitchStreamingSite = "https://www.twitch.tv/creepysin";
        public string GameMessage = "Use $help for help.";
        public bool IsAudioServiceEnabled = false;

        public bool CheckConnectionStatus = true;
        public int CheckConnectionStatusInterval = 60000;

		public int LevelUpCooldown = 15;

        //Bot APIs
        public ConfigApis Apis;

        //Help Modules
        public List<HelpModule> HelpModules = new List<HelpModule>();

        public class HelpModule
        {
            public string Group;
            public List<string> Modules = new List<string>();
        }
    }
}