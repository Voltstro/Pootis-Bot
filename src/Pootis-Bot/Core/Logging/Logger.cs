using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace Pootis_Bot.Core.Logging
{
	public static class Logger
	{
		private const string LogDirectory = "Logs/";

		private static string _finalLogName;

		private static bool _endLogger;

		private static StreamWriter _logStream;
		private static Thread _logThread;
		private static readonly ConcurrentQueue<string> Messages = new ConcurrentQueue<string>();

		/// <summary>
		/// Initializes the logger
		/// </summary>
		public static void InitiateLogger()
		{
			if (_logStream == null)
			{
				if (!Directory.Exists(LogDirectory))
					Directory.CreateDirectory(LogDirectory);

				_finalLogName = FinalLogName();

				//Create our StreamWriter
				_logStream = File.CreateText(LogDirectory + "latest.log");
				_logStream.AutoFlush = true;

				//Create a new thread for logging the messages
				_logThread = new Thread(WriteMessages)
				{
					Name = "LogThread",
					Priority = ThreadPriority.Lowest,
					IsBackground = true
				};
				_logThread.Start();
			}
			else
			{
				Log("The logger is already running! No need to initiate it multiple times!", LogVerbosity.Debug);
			}
		}

		/// <summary>
		/// Shuts down the logger
		/// </summary>
		public static void EndLogger()
		{
			_endLogger = true;
		}

		/// <summary>
		/// Logs a message
		/// </summary>
		/// <param name="message"></param>
		/// <param name="logVerbosity"></param>
		public static void Log(string message, LogVerbosity logVerbosity = LogVerbosity.Info)
		{
			if (_logStream == null)
			{
				throw new Exception("The log stream hasn't been setup yet!");
			}

			string formattedMessage = $"[{Global.TimeNow()} {logVerbosity}] {message}";

			if(logVerbosity != LogVerbosity.Debug)
				Messages.Enqueue(formattedMessage);
			else if(logVerbosity == LogVerbosity.Debug && Config.bot.LogDebugMessages)
				Messages.Enqueue(formattedMessage);

			//Write to the console depending on log verbosity and settings
			switch (logVerbosity)
			{
				case LogVerbosity.Info:
					WriteMessageToConsole(formattedMessage, ConsoleColor.White);
					break;
				case LogVerbosity.Debug:
					#if DEBUG
					Debug.WriteLine(formattedMessage);
					#endif
					if(Config.bot.LogDebugMessages)
						WriteMessageToConsole(formattedMessage, ConsoleColor.White);
					break;
				case LogVerbosity.Error:
					WriteMessageToConsole(formattedMessage, ConsoleColor.Red);
					break;
				case LogVerbosity.Warn:
					WriteMessageToConsole(formattedMessage, ConsoleColor.Yellow);
					break;
				case LogVerbosity.Music:
					WriteMessageToConsole(formattedMessage, ConsoleColor.Blue);
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(logVerbosity), logVerbosity, null);
			}
		}

		private static void WriteMessageToConsole(string message, ConsoleColor color)
		{
			Console.ForegroundColor = color;
			Console.WriteLine(message);
			Console.ForegroundColor = ConsoleColor.White;
		}

		private static void WriteMessages()
		{
			_endLogger = false;

			while (!_endLogger)
			{
				if(Messages.TryDequeue(out string message))
					WriteDirect(message);
			}

			while (!Messages.IsEmpty)
			{
				Messages.TryDequeue(out string message);
				WriteDirect(message);
			}

			WriteDirect($"Goodbye! Logger shutdown at {Global.TimeNow()} on day {DateTime.Now:MM-dd}");

			_logStream.Dispose();

			File.Copy(LogDirectory + "latest.log", LogDirectory + _finalLogName);

			static void WriteDirect(string message)
			{
				_logStream.WriteLine(message);
			}
		}

		private static string FinalLogName()
		{
			DateTime now = DateTime.Now;
			return $"{now:yyyy-MM-dd-HH-mm-ss}.log";
		}
	}
}
