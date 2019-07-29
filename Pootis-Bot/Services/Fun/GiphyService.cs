using System.Net;
using Newtonsoft.Json;
using Pootis_Bot.Core;
using Pootis_Bot.Structs;

namespace Pootis_Bot.Services.Fun
{
    public static class GiphyService
    {
        public static GiphySearchResult Search(string search)
        {
			GiphySearchResult searchResult = new GiphySearchResult();

            try
            {
                //Check to see if the token is null or white space
                if (!string.IsNullOrWhiteSpace(Config.bot.apis.apiGiphyKey))
                {
                    string input = search.Replace(" ", "+");

                    string json = "";
                    using (WebClient client = new WebClient()) //Search the term using the giphy api. More about the api here: https://developers.giphy.com/docs/
                    {
                        json = client.DownloadString($"http://api.giphy.com/v1/gifs/search?q={input}&api_key={Config.bot.apis.apiGiphyKey}");
                    }

                    var dataObject = JsonConvert.DeserializeObject<dynamic>(json);

                    int choose = Global.RandomNumber(0, 25);

                    GiphyData item = new GiphyData
                    {
                        gifUrl = dataObject.data[choose].images.fixed_height.url.ToString(),
                        gifTitle = dataObject.data[choose].title.ToString(),
                        gifAuthor = dataObject.data[choose].username.ToString(),
                        GifLink = dataObject.data[choose].bitly_gif_url.ToString()
                    };

					searchResult.IsSuccessfull = true;
					searchResult.Data = item;
					return searchResult;
                }
				else
				{
					searchResult.IsSuccessfull = false;
					searchResult.ErrorReason = ErrorReason.NoAPIKey;
					return searchResult;
				}
            }
            catch
            {
				searchResult.IsSuccessfull = false;
				searchResult.ErrorReason = ErrorReason.Error;
				return searchResult;
            } 
        }
    }
}
