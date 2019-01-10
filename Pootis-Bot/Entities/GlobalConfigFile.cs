namespace Pootis_Bot.Entities
{
    public class GlobalConfigFile
    {
        //Bot Related
        public string botToken = "";
        public string botPrefix = "";
        public string botName = "";

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