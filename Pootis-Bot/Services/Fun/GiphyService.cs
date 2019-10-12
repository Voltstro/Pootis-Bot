using System.Net;
using Newtonsoft.Json;
using Pootis_Bot.Core;
using Pootis_Bot.Structs;

namespace Pootis_Bot.Services.Fun
{
	public static class GiphyService
	{
		/// <summary>
		/// Searches giphy for a a given gif name
		/// </summary>
		/// <param name="search">The gif name to search for</param>
		/// <returns></returns>
		public static GiphySearchResult Search(string search)
		{
			GiphySearchResult searchResult = new GiphySearchResult();

			try
			{
				//Check to see if the token is null or white space
				if (!string.IsNullOrWhiteSpace(Config.bot.Apis.ApiGiphyKey))
				{
					string input = search.Replace(" ", "+");

					string json;
					using (WebClient client = new WebClient()) //Search the term using the giphy api. More about the api here: https://developers.giphy.com/docs/
					{
						json = client.DownloadString(
							$"http://api.giphy.com/v1/gifs/search?q={input}&api_key={Config.bot.Apis.ApiGiphyKey}");
					}

					dynamic dataObject = JsonConvert.DeserializeObject<dynamic>(json);

					int choose = Global.RandomNumber(0, 25);

					GiphyData item = new GiphyData
					{
						GifUrl = dataObject.data[choose].images.fixed_height.url.ToString(),
						GifTitle = dataObject.data[choose].title.ToString(),
						GifAuthor = dataObject.data[choose].username.ToString(),
						GifLink = dataObject.data[choose].bitly_gif_url.ToString()
					};

					searchResult.IsSuccessful = true;
					searchResult.Data = item;
					return searchResult;
				}

				searchResult.IsSuccessful = false;
				searchResult.ErrorReason = ErrorReason.NoApiKey;
				return searchResult;
			}
			catch
			{
				searchResult.IsSuccessful = false;
				searchResult.ErrorReason = ErrorReason.Error;
				return searchResult;
			}
		}
	}
}