using System;
using Pootis_Bot.Structs.Config;
using Console = Pootis_Bot.ConsoleCommandHandler.Console;

namespace Pootis_Bot.Core.ConfigMenuPlus
{
	public class ConfigApiMenu : Console
	{
		public void OpenApiMenu()
		{
			UnknownCommandError = "Unknown input! It either needs to be '1', '2', etc or 'return' to exit back to the main menu.";
			UnknownCommandErrorColor = ConsoleColor.Red;

			//Intro stuff
			System.Console.WriteLine("APIs are needed for commands such as `google`, `youtube`, `giphy`");
			System.Console.WriteLine("1 - Giphy API Key");
			System.Console.WriteLine("2 - Enable/Disable YouTube API functionality");
			System.Console.WriteLine("3 - Google API Key");
			System.Console.WriteLine("4 - Google Search Id");
			System.Console.WriteLine("5 - Steam API Key");
			System.Console.WriteLine("");
			System.Console.WriteLine("At any time type 'return' to return back to the bot configuration menu.");

			// -- Giphy Key
			AddCommand("1", "", EditGiphy);

			// -- YouTube
			AddCommand("2", "", SwitchYouTube);

			// -- Google Search Key
			AddCommand("3", "", EditGoogleKey);

			// -- Google Engine ID
			AddCommand("4", "", EditGoogleEngineId);

			// -- Steam Key
			AddCommand("5", "", EditSteamKey);

			// -- Return command
			AddCommand("return", "", () =>
			{
				IsExiting = true;
				System.Console.WriteLine("Returned back to config main menu.");
			});

			ConsoleHandleLoop();
		}

		private void EditGiphy()
		{
			ConsoleEdit.EditField<ConfigApis>(nameof(Config.bot.Apis.ApiGiphyKey), Config.bot.Apis);

			Config.SaveConfig();

			System.Console.WriteLine("The bot will now use the new Giphy API key provided.");
		}

		private void SwitchYouTube()
		{
			Config.bot.Apis.YouTubeService = !Config.bot.Apis.YouTubeService;
			Config.SaveConfig();

			System.Console.WriteLine(Config.bot.Apis.YouTubeService
				? "YouTube service is now enabled."
				: "YouTube service is now disabled.");
		}

		private void EditGoogleKey()
		{
			ConsoleEdit.EditField<ConfigApis>(nameof(Config.bot.Apis.ApiGoogleSearchKey), Config.bot.Apis);

			Config.SaveConfig();

			System.Console.WriteLine("The bot will now use the new Google Search API key provided.");
		}

		private void EditGoogleEngineId()
		{
			ConsoleEdit.EditField<ConfigApis>(nameof(Config.bot.Apis.GoogleSearchEngineId), Config.bot.Apis);

			Config.SaveConfig();

			System.Console.WriteLine("The bot will now use the new Google Engine ID provided.");
		}

		private void EditSteamKey()
		{
			ConsoleEdit.EditField<ConfigApis>(nameof(Config.bot.Apis.ApiSteamKey), Config.bot.Apis);

			Config.SaveConfig();

			System.Console.WriteLine("The bot will now use the new Steam API key provided on next restart.");
		}

		public override void LogMessage(string message, ConsoleColor color)
		{
			ConsoleColor currentColor = System.Console.ForegroundColor;
			System.Console.ForegroundColor = color;
			System.Console.WriteLine(message);
			System.Console.ForegroundColor = currentColor;
		}
	}
}