using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Pootis_Bot.ConsoleCommandHandler
{
	//Most of this is taken from Unity's FPS Sample console
	//Though a lot has been changed for Pootis-Bot's needs
	
	//Unity FPS sample project: https://github.com/Unity-Technologies/FPSSample

	public class Console
	{
		/// <summary>
		/// New console instance
		/// </summary>
		/// <param name="unknownCommandError"></param>
		/// <param name="unknownCommandErrorColor"></param>
		public Console(string unknownCommandError, ConsoleColor unknownCommandErrorColor)
		{
			_unknownCommandError = unknownCommandError;
			_unknownCommandErrorColor = unknownCommandErrorColor;

			Debug.WriteLine("[Console] Created a new console instance.");
		}

		private readonly Dictionary<string, ConsoleCommand> _consoleCommands = new Dictionary<string, ConsoleCommand>();
		private readonly string _unknownCommandError;
		private readonly ConsoleColor _unknownCommandErrorColor;

		/// <summary>
		/// Adds a new console command
		/// </summary>
		/// <param name="name"></param>
		/// <param name="summary"></param>
		/// <param name="method"></param>
		public void AddCommand(string name, string summary, Method method)
		{
			name = name.ToLower();
			if (_consoleCommands.ContainsKey(name))
			{
				Log($"The command {name} already exists!", ConsoleColor.Red);
				return;
			}

			_consoleCommands.Add(name, new ConsoleCommand(name, summary, method));
			Debug.WriteLine($"[Command] Added command {name}");
		}

		/// <summary>
		/// Executes the console command's method, if the command exists
		/// </summary>
		/// <param name="name"></param>
		public void ExecuteCommand(string name)
		{
			if (_consoleCommands.TryGetValue(name, out ConsoleCommand consoleCommand))
			{
				consoleCommand.Method();
			}
			else
			{
				Log(_unknownCommandError, _unknownCommandErrorColor);
			}
		}

		/// <summary>
		/// Starts a infinite console input loop
		/// </summary>
		public void ConsoleHandleLoop()
		{
			while (true)
			{
				string input = System.Console.ReadLine()?.Trim().ToLower();

				ExecuteCommand(input);
			}

			// ReSharper disable once FunctionNeverReturns
		}

		private static void Log(string message, ConsoleColor color = ConsoleColor.White)
		{
			System.Console.ForegroundColor = color;
			System.Console.WriteLine(message);
			System.Console.ForegroundColor = ConsoleColor.White;
		}
	}
}
