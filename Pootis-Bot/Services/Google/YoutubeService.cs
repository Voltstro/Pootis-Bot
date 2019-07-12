using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using Pootis_Bot.Core;

namespace Pootis_Bot.Services.Google
{
    public static class YoutubeService
    {
        public static SearchListResponse Search(string search, string appName)
        {
            return SearchYoutube(search, appName, 10);
        }

        public static SearchListResponse Search(string search, string appName, int maxResults)
        {
            return SearchYoutube(search, appName, maxResults);
        }

        private static SearchListResponse SearchYoutube(string search, string appName, int maxResults)
        {
            try
            {
                //Check to see if the token is null or white space
                if (!string.IsNullOrWhiteSpace(Config.bot.apis.apiYoutubeKey))
                {
                    var youtube = new YouTubeService(new BaseClientService.Initializer
                    {
                        ApiKey = Config.bot.apis.apiYoutubeKey,
                        ApplicationName = appName
                    });

                    var searchListRequest = youtube.Search.List("snippet");
                    searchListRequest.Q = search;
                    searchListRequest.MaxResults = maxResults;

                    //Search and return
                    return searchListRequest.Execute();
                }
                else
                    return null;
            }
            catch
            {
                return null;
            }
        }
    }
}
