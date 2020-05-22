using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Google.Apis.Customsearch.v1;
using Google.Apis.Services;
using Pootis_Bot.Core;
using Pootis_Bot.Core.Logging;

namespace Pootis_Bot.Services.Google.Search
{
	public class GoogleService : IGoogleSearcher
	{
		public async Task<List<GoogleSearch>> SearchGoogle(string search)
		{
			try
			{
				using CustomsearchService googleService = new CustomsearchService(new BaseClientService.Initializer
				{
					ApiKey = Config.bot.Apis.ApiGoogleSearchKey,
					ApplicationName = GetType().ToString()
				});

				CseResource.ListRequest searchRequest = googleService.Cse.List();
				searchRequest.Q = search;
				searchRequest.Cx = Config.bot.Apis.GoogleSearchEngineId;

				global::Google.Apis.Customsearch.v1.Data.Search googleSearch = await searchRequest.ExecuteAsync();

				List<GoogleSearch> searches = googleSearch.Items
					.Select(result => new GoogleSearch(result.Title, result.Snippet, result.Link)).ToList();
				return searches;
			}
			catch (Exception ex)
			{
#if DEBUG
				Logger.Log(ex.ToString(), LogVerbosity.Error);
#else
				Logger.Log(ex.Message, LogVerbosity.Error);
#endif
				return null;
			}
		}
	}
}