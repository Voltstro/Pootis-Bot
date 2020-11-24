using System;
using System.IO;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using JetBrains.Annotations;
using Pootis_Bot.Config;
using Pootis_Bot.Console;
using Pootis_Bot.Console.ConfigMenus;
using Pootis_Bot.Logging;
using Pootis_Bot.Modules;
using Pootis_Bot.Shared.Exceptions;

namespace Pootis_Bot.Core
{
	/// <summary>
	///     Main class for handling the bot
	/// </summary>
	public class Bot : IDisposable
	{
		private ModuleManager moduleManager;

		/// <summary>
		///     Whether or not this bot is running
		/// </summary>
		[PublicAPI]
		public bool IsRunning { get; private set; }

		/// <summary>
		///     The location of the application
		/// </summary>
		[PublicAPI]
		public static string ApplicationLocation { get; private set; }

		/// <summary>
		///		Client for interacting with Discord
		/// </summary>
		private DiscordSocketClient discordClient;

		/// <summary>
		///		Command handler
		/// </summary>
		private CommandHandler commandHandler;

		/// <summary>
		///		Config for the bot
		/// </summary>
		private BotConfig config;

		/// <summary>
		///     Disposes of this bot instance
		/// </summary>
		public void Dispose()
		{
			//The bot has already shutdown
			if (!IsRunning)
				throw new InitializationException("The bot has already shutdown!");

			ReleaseResources();
			GC.SuppressFinalize(this);
		}

		/// <summary>
		///     Runs the bot
		/// </summary>
		public async Task Run()
		{
			//Don't want to be-able to run the bot multiple times
			if (IsRunning)
				throw new InitializationException("A bot is already running!");

			ApplicationLocation = Path.GetDirectoryName(typeof(Bot).Assembly.Location);

			IsRunning = true;

			//Init the logger
			Logger.Init();
			Logger.Info("Starting bot...");

			//Get config
			config = Config<BotConfig>.Instance;

			//Load modules
			moduleManager = new ModuleManager("Modules/", "Assemblies/");
			moduleManager.LoadModules();

			//If the token is null or white space, open the config menu
			if (string.IsNullOrWhiteSpace(config.BotToken))
			{
				Logger.Error("The token in the config is null or empty! You must set it in the config menu.");
				OpenConfigMenu();
			}

			//Setup the discord client
			discordClient = new DiscordSocketClient(new DiscordSocketConfig
			{
				LogLevel = LogSeverity.Verbose
			});
			discordClient.Log += Log;
			discordClient.Ready += Ready;

			//Log in and start the Discord client
			Logger.Info("Logging into Discord bot...");
			try
			{
				await discordClient.LoginAsync(TokenType.Bot, config.BotToken);
				await discordClient.StartAsync();
			}
			catch (Discord.Net.HttpException)
			{
				Logger.Error("The supplied token was invalid!");
				Dispose();
				return;
			}

			Logger.Info("Login successful!");

			//Setup command handler
			commandHandler = new CommandHandler(discordClient);
			moduleManager.InstallDiscordModulesFromLoadedModules(commandHandler);
		}

		private Task Ready()
		{
			Logger.Info("Bot is now ready and online!");

			return Task.CompletedTask;
		}

		private Task Log(LogMessage message)
		{
			switch (message.Severity)
			{
				case LogSeverity.Critical:
				case LogSeverity.Error:
					Logger.Error(message.Message);
					break;
				case LogSeverity.Warning:
					Logger.Warn(message.Message);
					break;
				case LogSeverity.Info:
					Logger.Info(message.Message);
					break;
				case LogSeverity.Verbose:
				case LogSeverity.Debug:
					Logger.Debug(message.Message);
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}

			return Task.CompletedTask;
		}

		/// <summary>
		///     Starts a console loop
		/// </summary>
		public void ConsoleLoop()
		{
			ConsoleCommandManager.AddConsoleCommandsFromAssembly(typeof(Bot).Assembly);
			while (IsRunning)
			{
				string input = System.Console.ReadLine();
				if (input?.ToLower() == "exit" || input?.ToLower() == "quit")
					break;

				if (input == null)
					continue;

				ConsoleCommandManager.ExecuteCommand(input);
			}
		}

		~Bot()
		{
			ReleaseResources();
		}

		private void ReleaseResources()
		{
			discordClient.StopAsync().GetAwaiter().GetResult();
			discordClient.Dispose();

			moduleManager.Dispose();
			Logger.Shutdown();

			IsRunning = false;
		}

		[UsedImplicitly]
		[ConsoleCommand("config", "Opens the config menu for the bot")]
		private static void ConfigMenuCommand(string[] args)
		{
			OpenConfigMenu();
		}

		private static void OpenConfigMenu()
		{
			BotConfig config = Config<BotConfig>.Instance;
			ConsoleConfigMenu<BotConfig> configMenu = new ConsoleConfigMenu<BotConfig>(config);

			while (true)
			{
				configMenu.Show();

				if (string.IsNullOrWhiteSpace(config.BotToken))
				{
					Logger.Error("The bot token is not set! Set the bot token then exit out of the menu.");
					continue;
				}

				break;
			}

			config.Save();
		}
	}
}