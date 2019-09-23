using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using Pootis_Bot.Core;

namespace Pootis_Bot.Services.Google
{
	public static class YoutubeService
	{
		/// <summary>
		/// Searches youtube for a string
		/// </summary>
		/// <param name="search">The string to search for</param>
		/// <param name="appName"></param>
		/// <param name="maxResults">The maximum amount of results</param>
		/// <returns></returns>
		public static SearchListResponse Search(string search, string appName, int maxResults = 10)
		{
			return SearchYoutube(search, appName, maxResults);
		}
		/// <summary>
		/// Searches youtube for a string
		/// </summary>
		/// <param name="search">The string to search for</param>
		/// <param name="appName"></param>
		/// <param name="maxResults">The maximum amount of results</param>
		/// <returns></returns>
		private static SearchListResponse SearchYoutube(string search, string appName, int maxResults)
		{
			try
			{
				//Check to see if the token is null or white space
				if (!string.IsNullOrWhiteSpace(Config.bot.Apis.ApiYoutubeKey))
				{
					YouTubeService youtube = new YouTubeService(new BaseClientService.Initializer
					{
						ApiKey = Config.bot.Apis.ApiYoutubeKey,
						ApplicationName = appName
					});

					SearchResource.ListRequest searchListRequest = youtube.Search.List("snippet");
					searchListRequest.Q = search;
					searchListRequest.MaxResults = maxResults;

					//Search and return
					return searchListRequest.Execute();
				}

				return null;
			}
			catch
			{
				return null;
			}
		}
	}
}