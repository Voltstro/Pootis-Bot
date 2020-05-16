using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Pootis_Bot.Core.ConfigMenuPlus;
using Pootis_Bot.Core.Logging;
using Pootis_Bot.Core.Managers;
using Pootis_Bot.Events;
using Pootis_Bot.Services;

namespace Pootis_Bot.Core
{
	public class Bot
	{
		public static bool IsRunning;
		public static bool IsStreaming;
		private DiscordSocketClient _client;

		/// <summary>
		/// Starts the bot
		/// </summary>
		/// <returns></returns>
		public async Task StartBot()
		{
			IsStreaming = false;
			IsRunning = true;
			Global.BotStatusText = Config.bot.DefaultGameMessage;

			//Make sure the token isn't null or empty, if so open the bot config menu.
			if (string.IsNullOrEmpty(Global.BotToken)) new ConfigMainMenu().OpenConfigMenu();

			Logger.Log("Creating new client...", LogVerbosity.Debug);

			_client = new DiscordSocketClient(new DiscordSocketConfig
			{
				LogLevel = LogSeverity.Verbose
			});

			Logger.Log("Setting up events", LogVerbosity.Debug);

			//Setup client events
			_client.Log += Log;
			_client.Ready += BotReady;

			//Setup the remaining events
			EventsSetup unused = new EventsSetup(_client);

			Logger.Log("Signing in using token...", LogVerbosity.Debug);

			await _client.LoginAsync(TokenType.Bot,
				Global.BotToken); //Logging into the bot using the token in the config.
			await _client.StartAsync(); //Start the client

			Logger.Log("Sign in successful!", LogVerbosity.Debug);

			CommandHandler handler = new CommandHandler(_client);

			Logger.Log("Installing commands...", LogVerbosity.Debug);

			//Install all the Modules
			await handler.SetupCommandHandlingAsync();

			//Check all help modules
			HelpModulesManager.CheckHelpModules();

			//Bot owner
			Global.BotOwner = (await _client.GetApplicationInfoAsync()).Owner;

			Logger.Log($"The owner of this bot is {Global.BotOwner}", LogVerbosity.Debug);

			//Enable the Steam services if an api key is provided
			if (!string.IsNullOrWhiteSpace(Config.bot.Apis.ApiSteamKey))
				SteamService.SetupSteam();

			//Set the bot status to the default game status
			await _client.SetGameAsync(Config.bot.DefaultGameMessage);
			await CheckConnectionStatus();
		}

		private async Task BotReady()
		{
			//Check the current connected server settings
			await new BotCheckServerSettings(_client).CheckConnectedServerSettings();

			//Bot user
			Global.BotUser = _client.CurrentUser;

			Logger.Log("Bot is now ready and online!");

#pragma warning disable 4014
			Task.Run(() => new ConsoleCommandHandler(_client).SetupConsole());
#pragma warning restore 4014
		}

		private static Task Log(LogMessage msg)
		{
			Logger.Log(msg.Message);
			return Task.CompletedTask;
		}

		private async Task CheckConnectionStatus()
		{
			while (IsRunning)
			{
				if (Config.bot.CheckConnectionStatus
				) // It is enabled then check the connection status ever so milliseconds
				{
					await Task.Delay(Config.bot.CheckConnectionStatusInterval);

					Logger.Log("Checking bot connection status...", LogVerbosity.Debug);

					if (_client.ConnectionState != ConnectionState.Disconnected &&
					    (_client.ConnectionState != ConnectionState.Disconnecting || !IsRunning)) continue;

					Logger.Log("The bot had disconnect for some reason, restarting...", LogVerbosity.Warn);

					IsRunning = false;

					await _client.LogoutAsync();
					_client.Dispose();

					Logger.EndLogger();

					Global.HttpClient.Dispose();

					ProcessStartInfo newPootisStart = new ProcessStartInfo("dotnet", "Pootis-Bot.dll");
#pragma warning disable IDE0067 // Dispose objects before losing scope
					Process newPootis = new Process
					{
						StartInfo = newPootisStart
					};
#pragma warning restore IDE0067 // Dispose objects before losing scope
					newPootis.Start();
					Environment.Exit(0);
				}
				else
				{
					await Task.Delay(-1); // Just run forever
				}
			}
		}
	}
}