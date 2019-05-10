namespace Pootis_Bot.Entities
{
    public class GlobalConfigFile
    {
        public string botToken = "";
        public string botPrefix = "$";
        public string botName = "Bot";
        public string twichStreamingSite = "https://www.twitch.tv/creepysin";
        public string gameMessage = "Use $help for help.";
        public bool isAudioServiceEnabled = false;

        //Bot APIs
        public ConfigApis apis;

        public struct ConfigApis
        {
            public string apiGiphyKey;
            public string apiYoutubeKey;
            public string apiGoogleSearchKey;
            public string googleSearchEngineID;
        }
    }
}