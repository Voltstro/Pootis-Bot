using System;
using System.Threading.Tasks;
using Pootis_Bot.Core;
using Pootis_Bot.Services.Audio;

namespace Pootis_Bot
{
    public class Program
    {
        public static void Main(string[] args)
        => new Program().StartAsync(args).GetAwaiter().GetResult();

        public async Task StartAsync(string[] args)
        {
            Global.Log($"Starting...", ConsoleColor.White);

            string name = null, token = null, prefix = null;

			//This is just suggesting to use 64-bit
            if (!Environment.Is64BitOperatingSystem)
                Global.Log("This OS is a 32-bit os, 64-Bit is recommended!", ConsoleColor.Yellow);

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
            if (name == null) name = Config.bot.BotName;
            if (token == null) token = Config.bot.BotToken;
            if (prefix == null) prefix = Config.bot.BotPrefix;

			#endregion

			//Check the audio services, if they are enabled
			AudioCheckService.CheckAudioService();

            Console.Title = name + " Console";

			//Setup the bot, put in the name, prefix and token
            Bot bot = new Bot();
            Global.BotName = name;
            Global.BotPrefix = prefix;
            Global.BotToken = token;

            await bot.StartBot();
        }
    }
}
