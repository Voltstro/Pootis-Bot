using System;
using System.Reflection;
using Pootis_Bot.Core.Logging;
using Pootis_Bot.Exceptions;
using Pootis_Bot.Modules;

namespace Pootis_Bot.Core
{
	public class TestModule : IModule
	{
		public void Dispose()
		{
				
		}

		public ModuleInfo GetModuleInfo() => new ModuleInfo("TestModule", new Version(1, 0, 0));

		public void Init()
		{
				
		}
	}

	/// <summary>
	///     Main class for handling the bot
	/// </summary>
	public class Bot : IDisposable
	{
		public bool IsRunning { get; private set; }

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

			IsRunning = true;

			//Init the logger
			Logger.Init();
			Logger.Info("Starting bot...");

			new ModuleManager().LoadModules(Assembly.GetExecutingAssembly());
		}

		~Bot()
		{
			ReleaseResources();
		}

		private void ReleaseResources()
		{
			Logger.Shutdown();

			IsRunning = false;
		}
	}
}