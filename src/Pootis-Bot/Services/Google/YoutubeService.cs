using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using Pootis_Bot.Core;

namespace Pootis_Bot.Services.Google
{
	public static class YoutubeService
	{
		/// <summary>
		/// Searches YouTube
		/// </summary>
		/// <param name="search">The string to search for</param>
		/// <param name="appName"></param>
		/// <param name="maxResults">The maximum amount of results</param>
		/// <param name="searchType">The search type</param>
		/// <returns></returns>
		public static SearchListResponse Search(string search, string appName, int maxResults = 10, string searchType = "video,channel")
		{
			return SearchYoutube(search, appName, maxResults, searchType);
		}

		private static SearchListResponse SearchYoutube(string search, string appName, int maxResults, string searchType)
		{
			try
			{
				//Check to see if the token is null or white space
				if (string.IsNullOrWhiteSpace(Config.bot.Apis.ApiYoutubeKey)) return null;

				SearchListResponse youtubeSearch;

				using (YouTubeService youtube = new YouTubeService(new BaseClientService.Initializer
				{
					ApiKey = Config.bot.Apis.ApiYoutubeKey,
					ApplicationName = appName
				}))
				{
					SearchResource.ListRequest searchListRequest = youtube.Search.List("snippet");
					searchListRequest.Q = search;
					searchListRequest.MaxResults = maxResults;
					searchListRequest.Type = searchType;

					youtubeSearch = searchListRequest.Execute();
				}

				return youtubeSearch;
			}
			catch
			{
				return null;
			}
		}
	}
}