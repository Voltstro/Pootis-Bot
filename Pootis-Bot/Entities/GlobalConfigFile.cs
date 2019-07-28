using System.Collections.Generic;

namespace Pootis_Bot.Entities
{
    public class GlobalConfigFile
    {
        public string configVersion;

        public string botToken = "";
        public string botPrefix = "$";
        public string botName = "Bot";
        public string twitchStreamingSite = "https://www.twitch.tv/creepysin";
        public string gameMessage = "Use $help for help.";
        public bool isAudioServiceEnabled = false;

        public bool checkConnectionStatus = true;
        public int checkConnectionStatusInterval = 60000;

		public int levelUpCooldown = 15;

        //Bot APIs
        public ConfigApis apis;

        //Help modules
        public List<HelpModules> helpModules = new List<HelpModules>();

        public struct ConfigApis
        {
            public string apiGiphyKey;
            public string apiYoutubeKey;
            public string apiGoogleSearchKey;
            public string googleSearchEngineID;
        }

        public class HelpModules
        {
            public string group;
            public List<string> modules = new List<string>();
        }
    }
}