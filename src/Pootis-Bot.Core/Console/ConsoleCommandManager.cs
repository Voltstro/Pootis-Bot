using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Pootis_Bot.Logging;

namespace Pootis_Bot.Console
{
	/// <summary>
	///     Handles commands for the console
	/// </summary>
	public static class ConsoleCommandManager
	{
		public delegate void CommandArgumentsDelegate(string[] args);
		public delegate void CommandDelegate();

		private const BindingFlags BindingFlags = System.Reflection.BindingFlags.Static
		                                          | System.Reflection.BindingFlags.Public
		                                          | System.Reflection.BindingFlags.NonPublic;

		private static readonly Dictionary<string, CommandInfo> Commands = new Dictionary<string, CommandInfo>();

		/// <summary>
		///     Adds all <see cref="ConsoleCommand" /> found from an <see cref="Assembly" />
		/// </summary>
		/// <param name="assembly"></param>
		internal static void AddConsoleCommandsFromAssembly(Assembly assembly)
		{
			foreach (Type type in assembly.GetTypes())
			foreach (MethodInfo methodInfo in type.GetMethods(BindingFlags))
			{
				//Ignore if the field doesn't have the ConsoleCommand attribute, but if it does, get it
				if (!(Attribute.GetCustomAttribute(methodInfo, typeof(ConsoleCommand)) is ConsoleCommand attribute))
					continue;

				//Create the CommandArgumentsDelegate from the ConCommand's method
				CommandArgumentsDelegate commandArgumentsDelegate = null;
				CommandDelegate commandDelegate = null;
				try
				{
					if (methodInfo.GetParameters().Length == 0)
						commandDelegate =
							(CommandDelegate) Delegate.CreateDelegate(typeof(CommandDelegate), methodInfo);
					else
						commandArgumentsDelegate = (CommandArgumentsDelegate) Delegate.CreateDelegate(typeof(CommandArgumentsDelegate), methodInfo);
				}
				catch (Exception ex)
				{
					Logger.Error(ex, "An error occurred while adding the command {Command}'s method!",
						attribute.Command);
					continue;
				}

				if (Commands.ContainsKey(attribute.Command))
				{
					Logger.Error("The command {Command} already exists!", attribute.Command);
					continue;
				}

				Commands.Add(attribute.Command, new CommandInfo
				{
					CommandSummary = attribute.CommandSummary,
					CommandArgumentDel = commandArgumentsDelegate, 
					CommandDel = commandDelegate
				});

				Logger.Debug("Added command {Command}", attribute.Command);
			}
		}

		/// <summary>
		///     Executes a command
		/// </summary>
		/// <param name="command">The command and arguments to execute</param>
		/// <exception cref="ArgumentNullException"></exception>
		public static void ExecuteCommand([DisallowNull] string command)
		{
			if(string.IsNullOrWhiteSpace(command))
				throw new ArgumentNullException(nameof(command));

			List<string> tokens = Tokenize(command);
			if (tokens.Count < 1)
				return;

			if (Commands.TryGetValue(tokens[0].ToLower(), out CommandInfo conCommand))
			{
				//Get the arguments that were inputted
				string[] arguments = tokens.GetRange(1, tokens.Count - 1).ToArray();

				//Invoke the method
				try
				{
					if(conCommand.CommandArgumentDel != null)
						conCommand.CommandArgumentDel.Invoke(arguments);
					else
						conCommand.CommandDel.Invoke();
				}
				catch (Exception ex)
				{
					Logger.Error(ex, "An error occurred while invoking {Command}!", tokens[0].ToLower());
				}

				return;
			}

			Logger.Error("Unknown command: {command}.", tokens[0].ToLower());
		}

		/// <summary>
		///     Does the command exist in the command list?
		/// </summary>
		/// <param name="command"></param>
		/// <returns>Returns <c>true</c> if the command exists</returns>
		/// <exception cref="ArgumentNullException"></exception>
		public static bool DoesCommandExist([DisallowNull] string command)
		{
			if (string.IsNullOrWhiteSpace(command))
				throw new ArgumentNullException(nameof(command));

			return Commands.ContainsKey(command);
		}

		[ConsoleCommand("help", "Gets a list of all commands")]
		private static void HelpCommand()
		{
			foreach ((string command, CommandInfo commandInfo) in Commands)
				Logger.Info("`{Command}` - {Summary}", command, commandInfo.CommandSummary);
		}

		#region Argument Parsing

		private static List<string> Tokenize(string input)
		{
			int pos = 0;
			List<string> res = new List<string>();
			int c = 0;
			while (pos < input.Length && c++ < 10000)
			{
				SkipWhite(input, ref pos);
				if (pos == input.Length)
					break;

				if (input[pos] == '"' && (pos == 0 || input[pos - 1] != '\\'))
					res.Add(ParseQuoted(input, ref pos));
				else
					res.Add(Parse(input, ref pos));
			}

			return res;
		}

		private static void SkipWhite(string input, ref int pos)
		{
			while (pos < input.Length && " \t".IndexOf(input[pos]) > -1) pos++;
		}

		private static string ParseQuoted(string input, ref int pos)
		{
			pos++;
			int startPos = pos;
			while (pos < input.Length)
			{
				if (input[pos] == '"' && input[pos - 1] != '\\')
				{
					pos++;
					return input.Substring(startPos, pos - startPos - 1);
				}

				pos++;
			}

			return input[startPos..];
		}

		private static string Parse(string input, ref int pos)
		{
			int startPos = pos;
			while (pos < input.Length)
			{
				if (" \t".IndexOf(input[pos]) > -1) return input[startPos..pos];
				pos++;
			}

			return input[startPos..];
		}

		#endregion
	}
}