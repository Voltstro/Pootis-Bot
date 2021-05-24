using System;
using Pootis_Bot.Exceptions;
using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace Pootis_Bot.Logging
{
	/// <summary>
	///     Provides the ability to log stuff to a file and the console
	/// </summary>
	public static class Logger
	{
		private static Serilog.Core.Logger log;

		private static LoggerConfig loggerConfig;

		/// <summary>
		///     The logger's config, can only be set while the logger isn't running
		/// </summary>
		/// <exception cref="InitializationException"></exception>
		public static LoggerConfig LoggerConfig
		{
			set
			{
				if (IsLoggerInitialized)
					throw new InitializationException("The logger is already initialized!");

				loggerConfig = value;
			}
			get => loggerConfig;
		}

		/// <summary>
		///     Is the logger initialized?
		///     <para>Returns true if it is</para>
		/// </summary>
		public static bool IsLoggerInitialized => log != null;

		/// <summary>
		///		Is the logger in debug mode?
		/// </summary>
#if DEBUG
		public static bool DebugLogMode = true;
#else
		public static bool DebugLogMode;
#endif

		/// <summary>
		///     Initializes the logger
		/// </summary>
		/// <exception cref="InitializationException"></exception>
		internal static void Init()
		{
			if (IsLoggerInitialized)
				throw new InitializationException("The logger is already initialized!");

			LoggerConfig ??= new LoggerConfig();

			LoggingLevelSwitch level = new LoggingLevelSwitch
			{
#if DEBUG
				MinimumLevel = LogEventLevel.Debug
#endif
			};

			const string outPutTemplate = "{Timestamp:dd-MM hh:mm:ss tt} [{Level:u3}] {Message:lj}{NewLine}{Exception}";
			string logFileName =
				$"{loggerConfig.LogDirectory}{DateTime.Now.ToString(loggerConfig.LogFileDateTimeFormat)}.log";

			log = new LoggerConfiguration()
				.MinimumLevel.ControlledBy(level)
				.WriteTo.Console(outputTemplate: outPutTemplate)
				.WriteTo.Async(a => a.File(logFileName, outputTemplate: outPutTemplate,
					buffered: loggerConfig.BufferedFileWrite))
				.CreateLogger();

			log.Debug("Logger initialized at {Date}", DateTime.Now.ToString("dd/MM/yyyy hh:mm tt"));
		}

		/// <summary>
		///     Shuts down the logger
		/// </summary>
		/// <exception cref="InitializationException"></exception>
		internal static void Shutdown()
		{
			if (!IsLoggerInitialized)
				throw new InitializationException("The logger isn't initialized!");

			log.Debug("Logger shutting down at {Date}", DateTime.Now.ToString("dd/MM/yyyy hh:mm tt"));
			log.Dispose();
		}

		#region Debug Logging

		/// <summary>
		///     Writes a debug log
		/// </summary>
		/// <param name="message"></param>
		public static void Debug(string message)
		{
			if (!IsLoggerInitialized)
				throw new InitializationException("The logger isn't initialized!");

			if(DebugLogMode)
				log.Debug(message);
		}

		/// <summary>
		///     Writes a debug log
		/// </summary>
		/// <param name="message"></param>
		/// <param name="values"></param>
		public static void Debug(string message, params object[] values)
		{
			if (!IsLoggerInitialized)
				throw new InitializationException("The logger isn't initialized!");

			if(DebugLogMode)
				log.Debug(message, values);
		}

		#endregion

		#region Information Logging

		/// <summary>
		///     Writes an information log
		/// </summary>
		/// <param name="message"></param>
		public static void Info(string message)
		{
			if (!IsLoggerInitialized)
				throw new InitializationException("The logger isn't initialized!");

			log.Information(message);
		}

		/// <summary>
		///     Writes an information log
		/// </summary>
		/// <param name="message"></param>
		/// <param name="values"></param>
		public static void Info(string message, params object[] values)
		{
			if (!IsLoggerInitialized)
				throw new InitializationException("The logger isn't initialized!");

			log.Information(message, values);
		}

		#endregion

		#region Warning Logging

		/// <summary>
		///     Writes a warning log
		/// </summary>
		/// <param name="message"></param>
		public static void Warn(string message)
		{
			if (!IsLoggerInitialized)
				throw new InitializationException("The logger isn't initialized!");

			log.Warning(message);
		}

		/// <summary>
		///     Writes a warning log
		/// </summary>
		/// <param name="message"></param>
		/// <param name="values"></param>
		public static void Warn(string message, params object[] values)
		{
			if (!IsLoggerInitialized)
				throw new InitializationException("The logger isn't initialized!");

			log.Warning(message, values);
		}

		#endregion

		#region Error Logging

		/// <summary>
		///     Writes an error log
		/// </summary>
		/// <param name="message"></param>
		public static void Error(string message)
		{
			if (!IsLoggerInitialized)
				throw new InitializationException("The logger isn't initialized!");

			log.Error(message);
		}

		/// <summary>
		///     Writes an error log
		/// </summary>
		/// <param name="message"></param>
		/// <param name="values"></param>
		public static void Error(string message, params object[] values)
		{
			if (!IsLoggerInitialized)
				throw new InitializationException("The logger isn't initialized!");

			log.Error(message, values);
		}

		/// <summary>
		///     Writes an error log
		/// </summary>
		/// <param name="exception"></param>
		/// <param name="message"></param>
		public static void Error(Exception exception, string message)
		{
			if (!IsLoggerInitialized)
				throw new InitializationException("The logger isn't initialized!");

			log.Error(exception, message);
		}

		/// <summary>
		///     Writes an error log
		/// </summary>
		/// <param name="exception"></param>
		/// <param name="message"></param>
		/// <param name="values"></param>
		public static void Error(Exception exception, string message, params object[] values)
		{
			if (!IsLoggerInitialized)
				throw new InitializationException("The logger isn't initialized!");

			log.Error(exception, message, values);
		}

		#endregion
	}
}