namespace Pootis_Bot.Module.Upgrade.OldEntities.Server
{
    /// <summary>
    /// All of our API settings for 3rd party services are here (except for the Discord token)
    /// </summary>
    internal struct ConfigApis
    {
        /// <summary>
        /// Giphy API key
        /// </summary>
        public string ApiGiphyKey { get; set; }

        /// <summary>
        /// Google Search API key
        /// </summary>
        public string ApiGoogleSearchKey { get; set; }

        /// <summary>
        /// Google Search Engine ID
        /// </summary>
        public string GoogleSearchEngineId { get; set; }

        /// <summary>
        /// Steam API key
        /// </summary>
        public string ApiSteamKey { get; set; }

        /// <summary>
        /// Is the YouTube service enabled
        /// </summary>
        public bool YouTubeService { get; set; }
    }
}