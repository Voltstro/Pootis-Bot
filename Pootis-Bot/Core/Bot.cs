using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Pootis_Bot.Core.Managers;
using Pootis_Bot.Entities;
using Pootis_Bot.Events;
using Pootis_Bot.Services;
using Pootis_Bot.Services.Audio;

namespace Pootis_Bot.Core
{
	public class Bot
	{
		private DiscordSocketClient _client;

		private string _gameStatus = Config.bot.DefaultGameMessage;
		private bool _isRunning;
		private bool _isStreaming;

		/// <summary>
		/// Starts the bot
		/// </summary>
		/// <returns></returns>
		public async Task StartBot()
		{
			_isStreaming = false;
			_isRunning = true;

			//Make sure the token isn't null or empty, if so open the bot config menu.
			if (string.IsNullOrEmpty(Global.BotToken)) new ConfigMenu().OpenConfig(true);

			_client = new DiscordSocketClient(new DiscordSocketConfig
			{
				LogLevel = LogSeverity.Verbose
			});

			//Setup client events
			_client.Log += Log;
			_client.Ready += BotReady;

			//Setup the remaining events
			EventsSetup unused = new EventsSetup(_client);

			await _client.LoginAsync(TokenType.Bot,
				Global.BotToken); //Logging into the bot using the token in the config.
			await _client.StartAsync(); //Start the client
			CommandHandler handler = new CommandHandler(_client);

			//Install all the Modules
			await handler.InstallCommandsAsync();

			//Check all help modules
			handler.CheckHelpModules();

			//Bot owner
			Global.BotOwner = (await _client.GetApplicationInfoAsync()).Owner;

			//Set the bot status to the default game status
			await _client.SetGameAsync(_gameStatus);
			await CheckConnectionStatus();
		}

		private async Task BotReady()
		{
			//Check the current connected server settings
			await new BotCheckServerSettings(_client).CheckConnectedServerSettings();
			Global.Log("Bot is now ready and online!");

			ConsoleInput();
		}

		private static Task Log(LogMessage msg)
		{
			Global.Log(msg.Message);
			return Task.CompletedTask;
		}

		private async Task CheckConnectionStatus()
		{
			while (_isRunning)
				if (Config.bot.CheckConnectionStatus
				) // It is enabled then check the connection status ever so milliseconds
				{
					await Task.Delay(Config.bot.CheckConnectionStatusInterval);
					if (_client.ConnectionState != ConnectionState.Disconnected &&
					    (_client.ConnectionState != ConnectionState.Disconnecting || !_isRunning)) continue;

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

		private async void ConsoleInput()
		{
			while (true) // Run forever
			{
				// ReSharper disable once PossibleNullReferenceException
				string input = Console.ReadLine().Trim().ToLower();

				switch (input)
				{
					case "exit":
					{
						_isRunning = false;

						Global.Log("Shutting down...");
						await _client.SetGameAsync("Bot shutting down");
						foreach (ServerMusicItem channel in AudioService.currentChannels)
						{
							channel.IsExit = true;

							if (channel.FfMpeg != null)
							{
								channel.FfMpeg.Kill();
								channel.FfMpeg.Dispose();
							}

							//Just wait a moment
							await Task.Delay(100);

							await channel.AudioClient.StopAsync();

							channel.IsPlaying = false;
						}

						await _client.LogoutAsync();
						_client.Dispose();
						Environment.Exit(0);

						return;
					}
					case "config":
						new ConfigMenu().OpenConfig();
						break;
					case "about":
						Console.WriteLine(Global.aboutMessage);
						break;
					case "version":
						Console.WriteLine(Global.version);
						break;
					case "setgame":
					{
						Console.WriteLine("Enter in what you want to set the bot's game to: ");
						_gameStatus = Console.ReadLine();

						ActivityType activity = ActivityType.Playing;
						string twitch = null;
						if (_isStreaming)
						{
							activity = ActivityType.Streaming;
							twitch = Config.bot.TwitchStreamingSite;
						}

						await _client.SetGameAsync(_gameStatus, twitch, activity);

						Global.Log($"Bot's game was set to '{_gameStatus}'");
						break;
					}
					case "togglestream" when _isStreaming:
						_isStreaming = false;
						await _client.SetGameAsync(_gameStatus, "");
						Global.Log("Bot is no longer streaming");
						break;
					case "togglestream":
						_isStreaming = true;
						await _client.SetGameAsync(_gameStatus, Config.bot.TwitchStreamingSite, ActivityType.Streaming);
						Global.Log("Bot is streaming");
						break;
					case "deletemusic":
					{
						foreach (ServerMusicItem channel in AudioService.currentChannels)
						{
							channel.IsExit = true;

							if (channel.FfMpeg != null)
							{
								channel.FfMpeg.Kill();
								channel.FfMpeg.Dispose();
							}

							//Just wait a moment
							await Task.Delay(100);

							await channel.AudioClient.StopAsync();

							channel.IsPlaying = false;
						}

						AudioService.currentChannels.Clear();

						Global.Log("Deleting music directory...", ConsoleColor.Blue);
						if (Directory.Exists("Music/"))
						{
							Directory.Delete("Music/", true);
							Global.Log("Done!", ConsoleColor.Blue);
						}
						else
						{
							Global.Log("The music directory doesn't exist!", ConsoleColor.Blue);
						}

						break;
					}
					case "toggleaudio":
					{
						Config.bot.AudioSettings.AudioServicesEnabled = !Config.bot.AudioSettings.AudioServicesEnabled;
						Config.SaveConfig();

						Global.Log($"The audio service was set to {Config.bot.AudioSettings.AudioServicesEnabled}",
							ConsoleColor.Blue);
						if (Config.bot.AudioSettings.AudioServicesEnabled)
							AudioCheckService.CheckAudioService();
						break;
					}
					case "forceaudioupdate":
					{
						Global.Log("Updating audio files.", ConsoleColor.Blue);
						foreach (ServerMusicItem channel in AudioService.currentChannels)
							channel.AudioClient.Dispose();

						await Task.Delay(1000);

						//Delete old files first
						Directory.Delete("External/", true);
						File.Delete("libsodium.dll");
						File.Delete("opus.dll");

						AudioCheckService.UpdateAudioFiles();
						Global.Log("Audio files were updated.", ConsoleColor.Blue);
						break;
					}
					case "status":
						Global.Log(
							$"Bot status: {_client.ConnectionState.ToString()}\nServer count: {_client.Guilds.Count}\nLatency: {_client.Latency}");
						break;
					case "clear":
					case "cls":
						Console.Clear();
						break;
					case "resethelpmodules":
					case "resethelp":
						HelpModulesManager.ResetHelpModulesToDefault();
						HelpModulesManager.SaveHelpModules();

						Global.Log("The help modules were reset to there defaults.");
						break;
					case "save config":
						Config.SaveConfig();
						Global.Log("Config saved!");
						break;
					case "save accounts":
						UserAccountsManager.SaveAccounts();
						Global.Log("User accounts saved!");
						break;
					case "save servers":
						ServerListsManager.SaveServerList();
						Global.Log("Server list saved!");
						break;
					default:
						Global.Log(
							$"Unknown command '{input}'. Visit {Global.websiteConsoleCommands} for a list of console commands.",
							ConsoleColor.Red);
						break;
				}
			}
		}
	}
}