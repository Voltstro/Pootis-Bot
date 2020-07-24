using Pootis_Bot.Attributes;

namespace Pootis_Bot.Structs.Config
{
	/// <summary>
	/// All of our API settings for 3rd party services are here (except for the Discord token)
	/// </summary>
	public struct ConfigApis
	{
		/// <summary>
		/// Giphy API key
		/// </summary>
		[ConfigMenuName("Giphy API key")]
		public string ApiGiphyKey { get; set; }

		/// <summary>
		/// Google Search API key
		/// </summary>
		[ConfigMenuName("Google Search API key")]
		public string ApiGoogleSearchKey { get; set; }

		/// <summary>
		/// Google Search Engine ID
		/// </summary>
		[ConfigMenuName("Google Search Engine ID")]
		public string GoogleSearchEngineId { get; set; }

		/// <summary>
		/// Steam API key
		/// </summary>
		[ConfigMenuName("Steam API key")]
		public string ApiSteamKey { get; set; }

		/// <summary>
		/// Is the YouTube service enabled
		/// </summary>
		public bool YouTubeService { get; set; }
	}
}