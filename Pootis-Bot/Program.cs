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
			//Ascii art of Pootis-Bot because why not ¯\_(ツ)_/¯
			Console.WriteLine(@"__________              __  .__                 __________        __   ");
			Console.WriteLine(@"\______   \____   _____/  |_|__| ______         \______   \ _____/  |_ ");
			Console.WriteLine(@" |     ___/  _ \ /  _ \   __\  |/  ___/  ______  |    |  _//  _ \   __\");
			Console.WriteLine(@" |    |  (  <_> |  <_> )  | |  |\___ \  /_____/  |    |   (  <_> )  |  ");
			Console.WriteLine(@" |____|   \____/ \____/|__| |__/____  >          |______  /\____/|__|  ");
			Console.WriteLine(@"                                    \/                  \/             ");
			Console.WriteLine($"			Version: {Global.version}");
			Console.WriteLine();

            Global.Log("Starting...");

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

			//Start her up!
            await bot.StartBot();
        }
    }
}
