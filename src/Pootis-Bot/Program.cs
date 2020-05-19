using System;
using System.Net.Http;
using Pootis_Bot.Core;
using Pootis_Bot.Core.Logging;
using Pootis_Bot.Helpers;
using Pootis_Bot.Services.Audio.Music.ExternalLibsManagement;

//Pootis-Bot, Created by Voltstro (formally 'Creepysin') 2018-2020.
//Licensed under the MIT license
//
//Special Thanks:
// - EternalClickbait for initial XML comments and code cleanup
// - Mr. Fantastic Pootis ;D for suggesting the name and 'theme' of the bot
// - My parents (my mum for letting me sit on my ass all day and work on this, my dad for showing C# and letting me host the bot on his server)
// - Discord and Discord.NET for making this possible
// - And all the posts that I found on the internet that helped solved issues of mine

namespace Pootis_Bot
{
	public class Program
	{
		public static void Main()
		{
			Logger.InitiateLogger();

			//Ascii art of Pootis-Bot because why not ¯\_(ツ)_/¯
			Console.WriteLine(@"__________              __  .__                 __________        __   ");
			Console.WriteLine(@"\______   \____   _____/  |_|__| ______         \______   \ _____/  |_ ");
			Console.WriteLine(@" |     ___/  _ \ /  _ \   __\  |/  ___/  ______  |    |  _//  _ \   __\");
			Console.WriteLine(@" |    |  (  <_> |  <_> )  | |  |\___ \  /_____/  |    |   (  <_> )  |  ");
			Console.WriteLine(@" |____|   \____/ \____/|__| |__/____  >          |______  /\____/|__|  ");
			Console.WriteLine(@"                                    \/                  \/             ");
			Console.WriteLine($"			Version: {VersionUtils.GetAppVersion()}");
			Console.WriteLine();

			Logger.Log("Pootis-Bot starting...");

			//This is just suggesting to use 64-bit
			if (!Environment.Is64BitOperatingSystem)
				Logger.Log("This OS is a 32-bit os, 64-Bit is recommended!", LogVerbosity.Warn);

			Logger.Log("Creating the HttpClient object...", LogVerbosity.Debug);
			Global.HttpClient = new HttpClient();

			Logger.Log("Setting up global variable...", LogVerbosity.Debug);
			Global.BotName = Config.bot.BotName;
			Global.BotPrefix = Config.bot.BotPrefix;
			Global.BotToken = Config.bot.BotToken;

			//Check the audio services, if they are enabled
			MusicLibsChecker.CheckMusicService(false);

			Console.Title = $"{Global.BotName} Console";

			//Setup the bot, put in the name, prefix and token
			Logger.Log("Creating the bot instance...", LogVerbosity.Debug);
			Bot bot = new Bot();

			Logger.Log("Starting the bot...");

			//Start her up!
			bot.StartBot().GetAwaiter().GetResult();
		}
	}
}