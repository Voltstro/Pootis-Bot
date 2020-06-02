using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Pootis_Bot.Core.ConfigMenuPlus;
using Pootis_Bot.Core.Logging;
using Pootis_Bot.Core.Managers;
using Pootis_Bot.Entities;
using Pootis_Bot.Events;
using Pootis_Bot.Services;
using Pootis_Bot.Services.Audio.Music;

namespace Pootis_Bot.Core
{
	/// <summary>
	/// The main bot class
	/// </summary>
	public class Bot
	{
		public static bool IsRunning;
		public static bool IsStreaming;
		public DiscordSocketClient Client;

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

			Client = new DiscordSocketClient(new DiscordSocketConfig
			{
				LogLevel = LogSeverity.Verbose
			});

			Logger.Log("Setting up events", LogVerbosity.Debug);

			//Setup client events
			Client.Log += Log;
			Client.Ready += BotReady;

			//Setup the remaining events
			EventsSetup unused = new EventsSetup(Client);

			Logger.Log("Signing in using token...", LogVerbosity.Debug);

			await Client.LoginAsync(TokenType.Bot,
				Global.BotToken); //Logging into the bot using the token in the config.
			await Client.StartAsync(); //Start the client

			Logger.Log("Sign in successful!", LogVerbosity.Debug);

			CommandHandler handler = new CommandHandler(Client);

			Logger.Log("Installing commands...", LogVerbosity.Debug);

			//Install all the Modules
			await handler.SetupCommandHandlingAsync();

			//Check all help modules
			HelpModulesManager.CheckHelpModules();

			//Bot owner
			Global.BotOwner = (await Client.GetApplicationInfoAsync()).Owner;

			Logger.Log($"The owner of this bot is {Global.BotOwner}", LogVerbosity.Debug);

			//Enable the Steam services if an api key is provided
			if (!string.IsNullOrWhiteSpace(Config.bot.Apis.ApiSteamKey))
				SteamService.SetupSteam();

			//Set the bot status to the default game status
			await Client.SetGameAsync(Config.bot.DefaultGameMessage);

			//Starts the check connection status task, which will run indefinitely until the bot is stopped. 
			await CheckConnectionStatusTask();
		}

		/// <summary>
		/// Ends the bot
		/// </summary>
		public async Task EndBot()
		{
			await Client.SetGameAsync("Bot shutting down...");

			Logger.Log("Stopping audio services...", LogVerbosity.Music);
			foreach (ServerMusicItem channel in MusicService.currentChannels)
			{
				//If there is already a song playing, cancel it
				await MusicService.StopPlayingAudioOnServer(channel);

				//Just wait a moment
				await Task.Delay(100);

				await channel.AudioClient.StopAsync();

				Logger.Log($"Ended {channel.GuildId} audio session.", LogVerbosity.Debug);
			}

			//Stops the connecting checking task
			IsRunning = false;

			//Stop the bot client
			await Client.LogoutAsync();
			Client.Dispose();

			//Stop the global HttpClient
			Global.HttpClient.CancelPendingRequests();
			Global.HttpClient.Dispose();

			//End the logger
			Logger.EndLogger();
		}

		private async Task BotReady()
		{
			//Check the current connected server settings
			await new BotCheckServerSettings(Client).CheckConnectedServerSettings();

			//Bot user
			Global.BotUser = Client.CurrentUser;

			Logger.Log("Bot is now ready and online!");

#pragma warning disable 4014
			Task.Run(() => new ConsoleCommandHandler(this).SetupConsole());
#pragma warning restore 4014
		}

		private static Task Log(LogMessage msg)
		{
			Logger.Log(msg.Message);
			return Task.CompletedTask;
		}

		private async Task CheckConnectionStatusTask()
		{
			while (IsRunning)
			{
				if (Config.bot.CheckConnectionStatus) // It is enabled then check the connection status ever so milliseconds
				{
					await Task.Delay(Config.bot.CheckConnectionStatusInterval);

					Logger.Log("Checking bot connection status...", LogVerbosity.Debug);

					//Check the connection state
					if (Client.ConnectionState != ConnectionState.Disconnected &&
					    (Client.ConnectionState != ConnectionState.Disconnecting || !IsRunning)) continue;

					Logger.Log("The bot had disconnect for some reason, restarting...", LogVerbosity.Warn);

					await EndBot();

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