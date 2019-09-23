using Google.Apis.Customsearch.v1;
using Google.Apis.Customsearch.v1.Data;
using Google.Apis.Services;
using Pootis_Bot.Core;

namespace Pootis_Bot.Services.Google
{
	public static class GoogleService
	{
		/// <summary>
		/// Searches google for a string
		/// </summary>
		/// <param name="search"></param>
		/// <param name="appName"></param>
		/// <returns></returns>
		public static Search Search(string search, string appName)
		{
			try
			{
				if (!string.IsNullOrWhiteSpace(Config.bot.Apis.ApiGoogleSearchKey) &&
				    !string.IsNullOrWhiteSpace(Config.bot.Apis.GoogleSearchEngineId))
				{
					CustomsearchService google = new CustomsearchService(new BaseClientService.Initializer
					{
						ApiKey = Config.bot.Apis.ApiGoogleSearchKey,
						ApplicationName = appName
					});

					CseResource.ListRequest searchListRequest = google.Cse.List(search);
					searchListRequest.Cx = Config.bot.Apis.GoogleSearchEngineId;

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