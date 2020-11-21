using System;
using System.IO;
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
		public void Run()
		{
			//Don't want to be-able to run the bot multiple times
			if (IsRunning)
				throw new InitializationException("A bot is already running!");

			ApplicationLocation = Path.GetDirectoryName(typeof(Bot).Assembly.Location);

			IsRunning = true;

			//Init the logger
			Logger.Init();
			Logger.Info("Starting bot...");

			moduleManager = new ModuleManager("Modules/", "Assemblies/");
			moduleManager.LoadModules();
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
			moduleManager.Dispose();
			Logger.Shutdown();

			IsRunning = false;
		}

		[ConsoleCommand("config", "Opens the config menu for the bot")]
		private static void ConfigMenuCommand(string[] args)
		{
			ConsoleConfigMenu<BotConfig> configMenu = new ConsoleConfigMenu<BotConfig>(Config<BotConfig>.Instance);
			configMenu.Show();

			Config<BotConfig>.Instance.Save();
		}
	}
}