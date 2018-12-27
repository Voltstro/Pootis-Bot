using System;
using System.IO;
using Newtonsoft.Json;

namespace Pootis_Bot
{
    class Config
    {
        private const string configFolder = "Resources";
        private const string configFile = "config.json";

        public static ConfigFile bot;

        static Config()
        {
            if (!Directory.Exists(configFolder))        //Creates the Resources folder if it doesn't exist.
                Directory.CreateDirectory(configFolder);

            if (!File.Exists(configFolder + "/" + configFile))   //If the config.json file doesn't exist it creats a new one.
            {
                SaveConfig(null, null, null, null, null);
                Console.WriteLine("Config.json was created. Is this your first time runing?");
            }
            else
            {
                string json = File.ReadAllText(configFolder + "/" + configFile); //If it does exist then it continues like normal.
                bot = JsonConvert.DeserializeObject<ConfigFile>(json);
            }
        }

        public static void SaveConfig(string _token, string prefix, string name, string giphyyAPI, string ytAPI)
        {
            bot = new ConfigFile()
            {
                botToken = _token,
                botPrefix = prefix,
                botName = name,      
                apiGiphyKey = giphyyAPI,
                apiYoutubeKey = ytAPI
            };
            string json = JsonConvert.SerializeObject(bot, Formatting.Indented);
            File.WriteAllText(configFolder + "/" + configFile, json);
        }
    }
}