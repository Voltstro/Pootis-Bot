namespace Pootis_Bot.Structs.Config
{
	/// <summary>
	/// All of our API keys for 3rd party services are here (except Discord token)
	/// </summary>
	public struct ConfigApis
	{
		/// <summary>
		/// Giphy API key
		/// </summary>
		public string ApiGiphyKey { get; set; }

		/// <summary>
		/// YouTube API key
		/// </summary>
		public string ApiYoutubeKey;

		/// <summary>
		/// Google Search API key
		/// </summary>
		public string ApiGoogleSearchKey;

		/// <summary>
		/// Google Search Engine ID
		/// </summary>
		public string GoogleSearchEngineId;

		/// <summary>
		/// Steam API key
		/// </summary>
		public string ApiSteamKey;

		/// <summary>
		/// Is the YouTube service enabled
		/// </summary>
		public bool YouTubeService { get; set; }
	}
}