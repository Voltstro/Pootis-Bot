using System;
using System.IO;
using Newtonsoft.Json;
using Pootis_Bot.Entities;

namespace Pootis_Bot.Core
{
    public static class Config
    {
        private const string configFolder = "Resources";
        private const string configFile = "config.json";

        private const string configVersion = "3";

        public readonly static GlobalConfigFile bot = new GlobalConfigFile();

        static Config()
        {
            if (!Directory.Exists(configFolder))        //Creates the Resources folder if it doesn't exist.
                Directory.CreateDirectory(configFolder);

            if (!File.Exists(configFolder + "/" + configFile))   //If the config.json file doesn't exist it creats a new one.
            {
                bot.ConfigVersion = configVersion;
                AddHelpModuleDefaults();

                SaveConfig();

                Global.Log("Config.json was created. Is this your first time runing?", ConsoleColor.Yellow);
            }
            else
            {
                string json = File.ReadAllText(configFolder + "/" + configFile); //If it does exist then it continues like normal.
                bot = JsonConvert.DeserializeObject<GlobalConfigFile>(json);

                if (string.IsNullOrWhiteSpace(bot.ConfigVersion) || bot.ConfigVersion != configVersion)
                {
                    bot.ConfigVersion = configVersion;
                    SaveConfig();
                    Global.Log("Updated config to version " + configVersion, ConsoleColor.Yellow);
                }  
            }
        }

        public static void SaveConfig()
        {
            string json = JsonConvert.SerializeObject(bot, Formatting.Indented);
            File.WriteAllText(configFolder + "/" + configFile, json);
        }

        private static void AddHelpModuleDefaults()
        {
            var basic = new GlobalConfigFile.HelpModule
            {
                Group = "Basic"
            };
            basic.Modules.Add("BasicCommands");
            basic.Modules.Add("Misc");

            bot.HelpModules.Add(basic);

            var utils = new GlobalConfigFile.HelpModule
            {
                Group = "Utils"
            };
            utils.Modules.Add("Utils");

            bot.HelpModules.Add(utils);

            var fun = new GlobalConfigFile.HelpModule
            {
	            Group = "Fun"
            };
            fun.Modules.Add("GiphySearch");
            fun.Modules.Add("GoogleSearch");
            fun.Modules.Add("YoutubeSearch");
            fun.Modules.Add("TronaldDump");

            bot.HelpModules.Add(fun);

            var audio = new GlobalConfigFile.HelpModule
            {
	            Group = "Audio"
            };
            audio.Modules.Add("Music");

            bot.HelpModules.Add(audio);
        }
    }
}