using System;
using System.IO;
using Newtonsoft.Json;
using Pootis_Bot.Entities;

namespace Pootis_Bot.Core
{
    public static class Config
    {
        private const string ConfigFolder = "Resources";
        private const string ConfigFile = "config.json";

        private const string ConfigVersion = "3";

        public static readonly GlobalConfigFile bot = new GlobalConfigFile();

        static Config()
        {
            if (!Directory.Exists(ConfigFolder))        //Creates the Resources folder if it doesn't exist.
                Directory.CreateDirectory(ConfigFolder);

            if (!File.Exists(ConfigFolder + "/" + ConfigFile))   //If the config.json file doesn't exist it creats a new one.
            {
                bot.ConfigVersion = ConfigVersion;
                AddHelpModuleDefaults();

                SaveConfig();

                Global.Log("Config.json was created. Is this your first time runing?", ConsoleColor.Yellow);
            }
            else
            {
                string json = File.ReadAllText(ConfigFolder + "/" + ConfigFile); //If it does exist then it continues like normal.
                bot = JsonConvert.DeserializeObject<GlobalConfigFile>(json);

                if (string.IsNullOrWhiteSpace(bot.ConfigVersion) || bot.ConfigVersion != ConfigVersion)
                {
                    bot.ConfigVersion = ConfigVersion;
                    SaveConfig();
                    Global.Log("Updated config to version " + ConfigVersion, ConsoleColor.Yellow);
                }  
            }
        }

        public static void SaveConfig()
        {
            string json = JsonConvert.SerializeObject(bot, Formatting.Indented);
            File.WriteAllText(ConfigFolder + "/" + ConfigFile, json);
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