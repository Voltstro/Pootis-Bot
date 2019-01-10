using System;
using System.IO;
using Newtonsoft.Json;
using Pootis_Bot.Entities;

namespace Pootis_Bot
{
    class Config
    {
        private const string configFolder = "Resources";
        private const string configFile = "config.json";

        public static GlobalConfigFile bot;

        static Config()
        {
            if (!Directory.Exists(configFolder))        //Creates the Resources folder if it doesn't exist.
                Directory.CreateDirectory(configFolder);

            if (!File.Exists(configFolder + "/" + configFile))   //If the config.json file doesn't exist it creats a new one.
            {
                SaveConfig(null, null, null, null, null, null, null);
                Console.WriteLine("Config.json was created. Is this your first time runing?");
            }
            else
            {
                string json = File.ReadAllText(configFolder + "/" + configFile); //If it does exist then it continues like normal.
                bot = JsonConvert.DeserializeObject<GlobalConfigFile>(json);
            }
        }

        public static void SaveConfig(string _token, string prefix, string name, string giphyyAPI, string ytAPI, string gAPI, string gsSe)
        {
            bot = new GlobalConfigFile()
            {
                botToken = _token,
                botPrefix = prefix,
                botName = name      
            };
            bot.apis = new GlobalConfigFile.ConfigApis()
            {
                apiGiphyKey = giphyyAPI,
                apiYoutubeKey = ytAPI,
                apiGoogleSearchKey = gAPI,
                googleSearchEngineID = gsSe
            };

            string json = JsonConvert.SerializeObject(bot, Formatting.Indented);
            File.WriteAllText(configFolder + "/" + configFile, json);
        }
    }
}