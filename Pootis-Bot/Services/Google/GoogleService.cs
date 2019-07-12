using Google.Apis.Customsearch.v1;
using Google.Apis.Customsearch.v1.Data;
using Google.Apis.Services;
using Pootis_Bot.Core;

namespace Pootis_Bot.Services.Google
{
    public static class GoogleService
    {
        public static Search Search(string search, string appName)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(Config.bot.apis.apiGoogleSearchKey) &&
                !string.IsNullOrWhiteSpace(Config.bot.apis.googleSearchEngineID))
                {
                    var google = new CustomsearchService(new BaseClientService.Initializer
                    {
                        ApiKey = Config.bot.apis.apiGoogleSearchKey,
                        ApplicationName = appName
                    });

                    var searchListRequest = google.Cse.List(search);
                    searchListRequest.Cx = Config.bot.apis.googleSearchEngineID;

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
