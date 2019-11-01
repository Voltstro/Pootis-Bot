using Discord;

namespace Pootis_Bot.Modules.Fun
{
	/// <summary>
	/// Contains settings for fun commands
	/// </summary>
	public static class FunCmdsConfig
	{
		public static readonly string ytStartLink = "https://www.youtube.com/watch?v=";
		public static readonly string ytChannelStart = "https://www.youtube.com/channel/";

		public static readonly Color youtubeColor = new Color(229, 57, 38);
		public static readonly Color googleColor = new Color(53, 169, 84);
		public static readonly Color giphyColor = new Color(190, 101, 249);
		public static readonly Color trumpQuoteColor = new Color(245, 167, 59);
		public static readonly Color randomPersonColor = new Color(59, 245, 121);
		public static readonly Color wikipediaSearchColor = new Color(237, 237, 237);

		public static readonly int youtubeMaxSearches = 8;
		public static readonly int googleMaxSearches = 12;
		public static readonly int wikipediaMaxSearches = 15;
	}
}