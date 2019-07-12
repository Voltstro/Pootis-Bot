using System;
using System.Net;
using Newtonsoft.Json;
using Pootis_Bot.Core;

namespace Pootis_Bot.Services.Fun
{
    public class TronaldDumpService
    {
        public static string GetRandomQuote()
        {
            try
            {
                string json = "";
                using (WebClient client = new WebClient()) //Tronald Dump API
                {
                    json = client.DownloadString($"https://api.tronalddump.io/random/quote");
                }

                var dataObject = JsonConvert.DeserializeObject<dynamic>(json);

                return dataObject.value.ToString();
                
            }
            catch (Exception ex)
            {
                return "**ERROR**: " + ex.Message;
            }
        }

        public static string GetQuote(string search)
        {
            try
            {
                string json = "";
                using (WebClient client = new WebClient()) //Tronald Dump API
                {
                    json = client.DownloadString($"https://api.tronalddump.io/search/quote?query={search}");
                }

                var dataObject = JsonConvert.DeserializeObject<dynamic>(json);
                if(dataObject._embedded.quotes.Count == 0)
                {
                    return "No quotes found for that search!";
                }

                int index = Global.RandomNumber(0, dataObject._embedded.quotes.Count);

                string quote = dataObject._embedded.quotes[index].value.ToString();

                return quote;
            }
            catch (Exception ex)
            {
                return "**ERROR**: " + ex.Message;
            }
        }
    }
}
