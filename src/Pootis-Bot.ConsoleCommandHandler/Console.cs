using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Pootis_Bot.ConsoleCommandHandler
{
	//Most of this is taken from Unity's FPS Sample console
	//Though a lot has been changed for Pootis-Bot's needs

	//Unity FPS sample project: https://github.com/Unity-Technologies/FPSSample

	/// <summary>
	/// Provides a system to have a console with commands
	/// </summary>
	public abstract class Console
	{
		private readonly Dictionary<string, ConsoleCommand> consoleCommands = new Dictionary<string, ConsoleCommand>();

		public bool IsExiting;

		public string UnknownCommandError;
		public ConsoleColor UnknownCommandErrorColor;

		/// <summary>
		/// Creates a new <see cref="Console"/> instance
		/// </summary>
		protected Console()
		{
			Debug.WriteLine("[Console] Created a new console instance.");
		}

		/// <summary>
		/// Adds a new <see cref="ConsoleCommand"/> to the <see cref="consoleCommands"/> list
		/// </summary>
		/// <param name="name"></param>
		/// <param name="summary"></param>
		/// <param name="method"></param>
		public void AddCommand(string name, string summary, Method method)
		{
			name = name.ToLower();
			if (consoleCommands.ContainsKey(name))
			{
				LogMessage($"The command {name} already exists!", ConsoleColor.Red);
				return;
			}

			consoleCommands.Add(name, new ConsoleCommand(name, summary, method));
		}

		/// <summary>
		/// Returns all <see cref="ConsoleCommand"/>s installed in this <see cref="Console"/> instance
		/// </summary>
		/// <returns></returns>
		public Dictionary<string, ConsoleCommand> GetAllInstalledConsoleCommands()
		{
			return consoleCommands;
		}

		/// <summary>
		/// Executes the <see cref="ConsoleCommand"/>'s <see cref="Method"/>
		/// </summary>
		/// <param name="name"></param>
		public void ExecuteCommand(string name)
		{
			if (consoleCommands.TryGetValue(name, out ConsoleCommand consoleCommand))
				consoleCommand.Method();
			else
				LogMessage(UnknownCommandError, UnknownCommandErrorColor);
		}

		/// <summary>
		/// Starts an infinite console input loop, until <see cref="IsExiting"/> is set to true
		/// </summary>
		public void ConsoleHandleLoop()
		{
			while (!IsExiting)
			{
				string input = System.Console.ReadLine()?.Trim().ToLower();

				ExecuteCommand(input);
			}
		}

		/// <summary>
		/// Logs a message
		/// </summary>
		/// <param name="message"></param>
		/// <param name="color"></param>
		public abstract void LogMessage(string message, ConsoleColor color);
	}
}