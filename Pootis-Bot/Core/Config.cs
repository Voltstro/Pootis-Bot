using System;
using System.IO;
using Newtonsoft.Json;
using Pootis_Bot.Entities;

namespace Pootis_Bot.Core
{
    static class Config
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
                bot.configVersion = configVersion;
                AddHelpModuleDefaults();

                SaveConfig();

                Global.Log("Config.json was created. Is this your first time runing?", ConsoleColor.Yellow);
            }
            else
            {
                string json = File.ReadAllText(configFolder + "/" + configFile); //If it does exist then it continues like normal.
                bot = JsonConvert.DeserializeObject<GlobalConfigFile>(json);

                if (string.IsNullOrWhiteSpace(bot.configVersion) || bot.configVersion != configVersion)
                {
                    bot.configVersion = configVersion;
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
            var basic = new GlobalConfigFile.HelpModules
            {
                group = "Basic"
            };
            basic.modules.Add("BasicCommands");
            basic.modules.Add("Misc");

            bot.helpModules.Add(basic);

            var utils = new GlobalConfigFile.HelpModules
            {
                group = "Utils"
            };
            utils.modules.Add("Utils");

            bot.helpModules.Add(utils);

            var fun = new GlobalConfigFile.HelpModules
            {
                group = "Fun"
            };
            fun.modules.Add("GiphySearch");
            fun.modules.Add("GoogleSearch");
            fun.modules.Add("YoutubeSearch");
            fun.modules.Add("TronaldDump");

            bot.helpModules.Add(fun);

            var audio = new GlobalConfigFile.HelpModules
            {
                group = "Audio"
            };
            audio.modules.Add("Music");

            bot.helpModules.Add(audio);
        }
    }
}