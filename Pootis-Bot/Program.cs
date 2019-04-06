using System;
using System.IO;
using System.Threading.Tasks;
using Pootis_Bot.Core;

namespace Pootis_Bot
{
    internal class Program
    {
        static void Main(string[] args)
        => new Program().StartAsync(args).GetAwaiter().GetResult();

        public async Task StartAsync(string[] args)
        {
            Global.WriteMessage($"Starting...", ConsoleColor.White);

            string name = null, token = null, prefix = null;

            if (!Environment.Is64BitOperatingSystem)
                Global.WriteMessage("This OS is a 32-bit os, 64-Bit is recommended!", ConsoleColor.Yellow);

            #region Config Loading

            //Check config, if there arguments use them as the name, token and prefix
            if (args.Length != 0)
            {
                if(args.Length == 1)
                    token = args[0];
                if (args.Length == 2)
                    prefix = args[1];
                if (args.Length == 3)
                    name = args[2];
            }
            if (name == null) name = Config.bot.botName;
            if (token == null) token = Config.bot.botToken;
            if (prefix == null) prefix = Config.bot.botPrefix;

            #endregion

            //Check the audio services, if they are enabled
            CheckAudioService();

            Console.Title = name + " Console";

            Bot bot = new Bot(token, name, prefix);

            await bot.StartBot();
        }

        public static void CheckAudioService()
        {
            if (Config.bot.isAudioServiceEnabled)
            {
                Global.WriteMessage("Checking audio services...", ConsoleColor.Blue);

                if (!File.Exists(@"External/ffmpeg.exe"))
                {
                    Global.WriteMessage("You are missing ffmpeg 32-bit!", ConsoleColor.Red);
                    Config.bot.isAudioServiceEnabled = false;
                    Config.SaveConfig();
                    Global.WriteMessage("Audio service was disabled!", ConsoleColor.Red);
                }
                else if (!File.Exists(@"External/ffprobe.exe"))
                {
                    Global.WriteMessage("You are missing ffprobe 32-bit!", ConsoleColor.Red);
                    Config.bot.isAudioServiceEnabled = false;
                    Config.SaveConfig();
                    Global.WriteMessage("Audio service was disabled!", ConsoleColor.Red);
                }
                else if (!File.Exists(@"External/ffplay.exe"))
                {
                    Global.WriteMessage("You are missing ffplay 32-bit!", ConsoleColor.Red);
                    Config.bot.isAudioServiceEnabled = false;
                    Config.SaveConfig();
                    Global.WriteMessage("Audio service was disabled!", ConsoleColor.Red);
                }
                else if (string.IsNullOrWhiteSpace( Config.bot.apis.apiYoutubeKey))
                {
                    Global.WriteMessage("You need to set a YouTube api key! You can get one from https://console.developers.google.com and creating a new project with the YouTube Data API v3", ConsoleColor.Red);
                    Config.bot.isAudioServiceEnabled = false;
                    Config.SaveConfig();
                    Global.WriteMessage("Audio service was disabled!", ConsoleColor.Red);
                }
                else
                    Global.WriteMessage("Audio services are ready", ConsoleColor.Blue);
            }
        }
    }
}
