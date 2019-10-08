using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Pootis_Bot.Entities;
using Pootis_Bot.Events;
using Pootis_Bot.Services;
using Pootis_Bot.Services.Audio;
using Pootis_Bot.Structs;

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
			if (string.IsNullOrEmpty(Global.BotToken)) BotConfigStart();

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

			//Bot owner
			Global.BotOwner = (await _client.GetApplicationInfoAsync()).Owner;

			//Set the bot status to the default game status
			await _client.SetGameAsync(_gameStatus);
			await CheckConnectionStatus();
		}

		private async Task BotReady()
		{
			//Check the current connected server settings
			await BotCheckServerSettings.CheckConnectedServerSettings(_client);
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
				if (Config.bot.CheckConnectionStatus) // It is enabled then check the connection status ever so milliseconds
				{
					await Task.Delay(Config.bot.CheckConnectionStatusInterval);
					if ((_client.ConnectionState == ConnectionState.Disconnected) ||
					    ((_client.ConnectionState == ConnectionState.Disconnecting) && _isRunning))
					{
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
						foreach (GlobalServerMusicItem channel in AudioService.currentChannels)
							channel.AudioClient.Dispose();

						await _client.LogoutAsync();
						_client.Dispose();
						Environment.Exit(0);

						return;
					}
					case "config":
						BotConfigStart();
						Global.Log("Restart the bot to apply the settings");
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
						foreach (GlobalServerMusicItem channel in AudioService.currentChannels)
							channel.AudioClient.Dispose();

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
						Config.bot.IsAudioServiceEnabled = !Config.bot.IsAudioServiceEnabled;
						Config.SaveConfig();

						Global.Log($"The audio service was set to {Config.bot.IsAudioServiceEnabled}",
							ConsoleColor.Blue);
						if (Config.bot.IsAudioServiceEnabled)
							AudioCheckService.CheckAudioService();
						break;
					}
					case "forceaudioupdate":
					{
						Global.Log("Updating audio files.", ConsoleColor.Blue);
						foreach (GlobalServerMusicItem channel in AudioService.currentChannels)
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
						Config.bot.HelpModules.Clear();
						Config.AddHelpModuleDefaults();

						Config.SaveConfig();

						Global.Log("The help modules were reset to there defaults.");
						break;
					default:
						Global.Log(
							$"Unknown command '{input}'. Vist {Global.websiteConsoleCommands} for a list of console commands.",
							ConsoleColor.Red);
						break;
				}
			}
		}

		#region Bot Config

		/// <summary>
		/// Starts the bot config
		/// </summary>
		public void BotConfigStart()
		{
			Console.WriteLine("");
			Console.WriteLine("---------------------------------------------------------");
			Console.WriteLine("                    Bot configuration                    ");
			Console.WriteLine("---------------------------------------------------------");
			Console.WriteLine("1. - Bot Token");
			Console.WriteLine("2. - Bot Prefix");
			Console.WriteLine("3. - Bot Name");
			Console.WriteLine("4. - APIs");
			Console.WriteLine("");
			Console.WriteLine("At any time type 'exit' to exit the bot configuration");
			BotConfigMain();
		}

		private void BotConfigMain()
		{
			string token = Global.BotToken;
			string name = Global.BotName;
			string prefix = Global.BotPrefix;

			while (true)
			{
				// ReSharper disable once PossibleNullReferenceException
				string input = Console.ReadLine().Trim();

				switch (input)
				{
					case "exit" when !string.IsNullOrEmpty(token) && !string.IsNullOrEmpty(name) &&
					                 !string.IsNullOrEmpty(prefix):
						Config.bot.BotToken = token;
						Config.bot.BotName = name;
						Config.bot.BotPrefix = prefix;

						Config.SaveConfig();

						Global.BotToken = token;
						Global.BotName = name;
						Global.BotPrefix = prefix;

						Console.WriteLine("Exited bot configuration");
						return;
					case "exit":
						Console.WriteLine(
							"Either the token, name or prefix is null or empty. Make sure to check that the all have data in it.");
						break;
					case "1":
						token = BotConfigToken();
						break;
					case "2":
						prefix = BotConfigPrefix();
						break;
					case "3":
						name = BotConfigName();
						break;
					case "4":
						BotConfigApis();
						break;
					default:
						Console.WriteLine(
							"Invaild input, you need to either enter '1', '2', '3', '4' or 'exit' (With out '')");
						break;
				}
			}
		}

		private void BotConfigApis()
		{
			string giphyApi = Config.bot.Apis.ApiGiphyKey;
			string youTubeApi = Config.bot.Apis.ApiYoutubeKey;
			string googleApi = Config.bot.Apis.ApiGoogleSearchKey;
			string googleSearchApi = Config.bot.Apis.GoogleSearchEngineId;

			Console.WriteLine("APIs are needed for commands such as 'google'");
			Console.WriteLine("It is definitely recommended.");
			Console.WriteLine("");
			Console.WriteLine("1. - Giphy API Key");
			Console.WriteLine("2. - Youtube API Key");
			Console.WriteLine("3. - Google API Key");
			Console.WriteLine("4. - Google Search Id");
			Console.WriteLine("");
			Console.WriteLine("At any time type 'return' to return back to the bot configuration menu.");

			while (true)
			{
				// ReSharper disable once PossibleNullReferenceException
				string input = Console.ReadLine().Trim();

				if (input.ToLower() == "return")
				{
					Config.bot.Apis.ApiGiphyKey = giphyApi;
					Config.bot.Apis.ApiYoutubeKey = youTubeApi;
					Config.bot.Apis.ApiGoogleSearchKey = googleApi;
					Config.bot.Apis.GoogleSearchEngineId = googleSearchApi;

					Config.SaveConfig();
					Console.WriteLine("Exited api configuration");
					return;
				}

				switch (input)
				{
					case "1":
						giphyApi = BotConfigAPIGiphy();
						break;
					case "2":
						youTubeApi = BotConfigAPIYoutube();
						break;
					case "3":
						googleApi = BotConfigAPIGoogle();
						break;
					case "4":
						googleSearchApi = BotConfigGoogleSearchID();
						break;
					default:
						Console.WriteLine("You need to either put in '1', '2' ... etc or 'return'. (With out '')");
						break;
				}
			}
		}

		#region Inputs

		private string BotConfigToken()
		{
			Console.WriteLine($"The current bot token is set to: '{Config.bot.BotToken}'");
			Console.WriteLine("Enter in what you want to change the bot token to: ");

			while (true)
			{
				string token = Console.ReadLine();
				if (string.IsNullOrWhiteSpace(token))
				{
					Console.WriteLine("You cannot set the bot token to blank!");
				}
				else
				{
					Console.WriteLine($"Bot's token was set to '{token}'");
					return token;
				}
			}
		}

		private string BotConfigPrefix()
		{
			Console.WriteLine($"The current bot prefix is set to: '{Config.bot.BotPrefix}'");
			Console.WriteLine("Enter in what you want to change the bot prefix to: ");

			while (true)
			{
				string prefix = Console.ReadLine();
				if (string.IsNullOrWhiteSpace(prefix))
				{
					Console.WriteLine("You cannot set the bot prefix to blank!");
				}
				else
				{
					Console.WriteLine($"Bot's prefix was set to '{prefix}'");
					return prefix;
				}
			}
		}

		private string BotConfigName()
		{
			Console.WriteLine($"The current bot name is set to: '{Config.bot.BotName}'");
			Console.WriteLine("Enter in what you want to change the bot name to: ");

			while (true)
			{
				string name = Console.ReadLine();
				if (string.IsNullOrWhiteSpace(name))
				{
					Console.WriteLine("You cannot set the bot name to blank!");
				}
				else
				{
					Console.WriteLine($"Bot's name was set to '{name}'");
					return name;
				}
			}
		}

		private string BotConfigAPIGiphy()
		{
			Console.WriteLine($"The current bot Giphy key is set to: '{Config.bot.Apis.ApiGiphyKey}'");
			Console.WriteLine("Enter in what you want to change the bot Giphy key to: ");

			while (true)
			{
				string key = Console.ReadLine();
				if (string.IsNullOrWhiteSpace(key))
				{
					Console.WriteLine("You cannot set the bot Giphy key to blank!");
				}
				else
				{
					Console.WriteLine($"Bot's Giphy key was set to '{key}'");
					return key;
				}
			}
		}

		private string BotConfigAPIYoutube()
		{
			Console.WriteLine($"The current bot Youtube key is set to: '{Config.bot.Apis.ApiYoutubeKey}'");
			Console.WriteLine("Enter in what you want to change the bot Youtube key to: ");

			while (true)
			{
				string key = Console.ReadLine();
				if (string.IsNullOrWhiteSpace(key))
				{
					Console.WriteLine("You cannot set the bot Youtube key to blank!");
				}
				else
				{
					Console.WriteLine($"Bot's Youtube key was set to '{key}'");
					return key;
				}
			}
		}

		private string BotConfigAPIGoogle()
		{
			Console.WriteLine($"The current bot Google key is set to: '{Config.bot.Apis.ApiGoogleSearchKey}'");
			Console.WriteLine("Enter in what you want to change the bot Google key to: ");

			while (true)
			{
				string key = Console.ReadLine();
				if (string.IsNullOrWhiteSpace(key))
				{
					Console.WriteLine("You cannot set the bot Google key to blank!");
				}
				else
				{
					Console.WriteLine($"Bot's Google key was set to '{key}'");
					return key;
				}
			}
		}

		private string BotConfigGoogleSearchID()
		{
			Console.WriteLine($"The current bot Google Search Id is set to: '{Config.bot.Apis.GoogleSearchEngineId}'");
			Console.WriteLine("Enter in what you want to change the bot Google Search Id to: ");

			while (true)
			{
				string key = Console.ReadLine();
				if (string.IsNullOrWhiteSpace(key))
				{
					Console.WriteLine("You cannot set the bot Google Search Id to blank!");
				}
				else
				{
					Console.WriteLine($"Bot's Google Search Id was set to '{key}'");
					return key;
				}
			}
		}

		#endregion

		#endregion
	}
}