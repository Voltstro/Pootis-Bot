using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
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
			if (string.IsNullOrEmpty(Global.BotToken)) new ConfigMenu().OpenConfig(true);

			Debug.WriteLine("[Bot] Creating new client...");

			_client = new DiscordSocketClient(new DiscordSocketConfig
			{
				LogLevel = LogSeverity.Verbose
			});

			//Setup client events
			_client.Log += Log;
			_client.Ready += BotReady;

			AppDomain.CurrentDomain.ProcessExit += QuitEvent;

			Debug.WriteLine("[Bot] Setting up events");

			//Setup the remaining events
			EventsSetup unused = new EventsSetup(_client);

			Debug.WriteLine("[Bot] Signing in using token...");

			await _client.LoginAsync(TokenType.Bot,
				Global.BotToken); //Logging into the bot using the token in the config.
			await _client.StartAsync(); //Start the client

			Debug.WriteLine("[Bot] Sign in successful");

			CommandHandler handler = new CommandHandler(_client);

			Debug.WriteLine("[Bot] Installing commands...");

			//Install all the Modules
			await handler.SetupAsync();

			//Check all help modules
			handler.CheckHelpModules();

			//Bot owner
			Global.BotOwner = (await _client.GetApplicationInfoAsync()).Owner;

			//Bot user
			Global.BotUser = _client.CurrentUser;

			Debug.WriteLine($"[Bot] The owner of this bot is {Global.BotOwner}");

			//Set the bot status to the default game status
			await _client.SetGameAsync(Config.bot.DefaultGameMessage);
			await CheckConnectionStatus();
		}

		private async Task BotReady()
		{
			//Check the current connected server settings
			await new BotCheckServerSettings(_client).CheckConnectedServerSettings();
			Global.Log("Bot is now ready and online!");

			new ConsoleCommandHandler(_client).SetupConsole();
		}

		private static Task Log(LogMessage msg)
		{
			Global.Log(msg.Message);
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

					Debug.WriteLine("[Bot Connection] Checking bot connection status...");

					if (_client.ConnectionState != ConnectionState.Disconnected &&
					    (_client.ConnectionState != ConnectionState.Disconnecting || !IsRunning)) continue;

					Global.Log("The bot had disconnect for some reason, restarting...", ConsoleColor.Yellow);

					await _client.LogoutAsync();
					_client.Dispose();

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

		private async void QuitEvent(object sender, EventArgs e)
		{
			IsRunning = false;

			Global.HttpClient.Dispose();

			if (_client == null) return;

			await _client.LogoutAsync();

			_client.Dispose();
		}
	}
}