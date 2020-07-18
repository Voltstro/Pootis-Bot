using System;
using Newtonsoft.Json;
using Pootis_Bot.Core;
using Pootis_Bot.Core.Logging;
using Pootis_Bot.Helpers;

namespace Pootis_Bot.Services.Fun
{
	public class TronaldDumpService
	{
		/// <summary>
		/// Gets a random donald trump quote
		/// </summary>
		/// <returns></returns>
		public static string GetRandomQuote()
		{
			try
			{
				string json = WebUtils.DownloadString("https://api.tronalddump.io/random/quote", "accept",
					"application/hal+json");

				dynamic dataObject = JsonConvert.DeserializeObject<dynamic>(json);

				return dataObject.value.ToString();
			}
			catch (Exception ex)
			{
				Logger.Error("Error occured while trying to get random Trump Quote: {@Exception}", ex);
				return "**ERROR**: " + ex.Message;
			}
		}

		public static string GetQuote(string search)
		{
			try
			{
				string json = WebUtils.DownloadString($"https://api.tronalddump.io/search/quote?query={search}",
					"accept", "application/hal+json");

				dynamic dataObject = JsonConvert.DeserializeObject<dynamic>(json);
				if (dataObject._embedded.quotes.Count == 0) return "No quotes found for that search!";

				int index = Global.RandomNumber(0, dataObject._embedded.quotes.Count);

				string quote = dataObject._embedded.quotes[index].value.ToString();

				return quote;
			}
			catch (Exception ex)
			{
				Logger.Error("Error occured while trying to get Trump Quote: {@Exception}", ex);
				return "**ERROR**: " + ex.Message;
			}
		}
	}
}