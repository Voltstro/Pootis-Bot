using System;
using System.IO;
using Newtonsoft.Json;
using Pootis_Bot.Core;
using Pootis_Bot.Entities;

namespace Pootis_Bot
{
    class Config
    {
        private const string configFolder = "Resources";
        private const string configFile = "config.json";

        public static GlobalConfigFile bot = new GlobalConfigFile();

        static Config()
        {
            if (!Directory.Exists(configFolder))        //Creates the Resources folder if it doesn't exist.
                Directory.CreateDirectory(configFolder);

            if (!File.Exists(configFolder + "/" + configFile))   //If the config.json file doesn't exist it creats a new one.
            {
                SaveConfig();

                Global.WriteMessage("Config.json was created. Is this your first time runing?", ConsoleColor.Yellow);
            }
            else
            {
                LoadConfig();
            }
        }

        public static void LoadConfig()
        {
            string json = File.ReadAllText(configFolder + "/" + configFile); //If it does exist then it continues like normal.
            bot = JsonConvert.DeserializeObject<GlobalConfigFile>(json);
        }

        public static void SaveConfig()
        {
            string json = JsonConvert.SerializeObject(bot, Formatting.Indented);
            File.WriteAllText(configFolder + "/" + configFile, json);
        }
    }
}