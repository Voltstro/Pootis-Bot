using System;
using System.Collections.Generic;
using System.Reflection;
using JetBrains.Annotations;
using Pootis_Bot.Logging;

namespace Pootis_Bot.Console
{
	/// <summary>
	///		Handles commands for the console
	/// </summary>
	public static class ConsoleCommandManager
	{
		public delegate void MethodDelegate(string[] args);

		private const BindingFlags BindingFlags = System.Reflection.BindingFlags.Static 
		                                          | System.Reflection.BindingFlags.Public 
		                                          | System.Reflection.BindingFlags.NonPublic;

		private static readonly Dictionary<string, CommandInfo> commands = new Dictionary<string, CommandInfo>();

		/// <summary>
		///		Adds all <see cref="ConsoleCommand"/> found from an <see cref="Assembly"/>
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

				//Create the MethodDelegate from the ConCommand's method
				MethodDelegate methodDelegate;
				try
				{
					methodDelegate = (MethodDelegate) Delegate.CreateDelegate(typeof(MethodDelegate), methodInfo);
				}
				catch (Exception ex)
				{
					Logger.Error("An error occurred while adding the command `{@Command}`'s method! {@Exception}", attribute.Command, ex);
					continue;
				}

				if (commands.ContainsKey(attribute.Command))
				{
					Logger.Error("The command {@Command} already exists!", attribute.Command);
					continue;
				}

				commands.Add(attribute.Command, new CommandInfo
				{
					CommandSummary = attribute.CommandSummary,
					Method = methodDelegate
				});

				Logger.Debug("Added command {@Command}", attribute.Command);
			}
		}

		/// <summary>
		///		Executes a command
		/// </summary>
		/// <param name="command">The command and arguments to execute</param>
		[PublicAPI]
		public static void ExecuteCommand([NotNull] string command)
		{
			List<string> tokens = Tokenize(command);
			if (tokens.Count < 1)
				return;

			if (commands.TryGetValue(tokens[0].ToLower(), out CommandInfo conCommand))
			{
				//Get the arguments that were inputted
				string[] arguments = tokens.GetRange(1, tokens.Count - 1).ToArray();

				//Invoke the method
				try
				{
					conCommand.Method.Invoke(arguments);
				}
				catch (Exception ex)
				{
					Logger.Error("An error occurred! {@Exception}", ex);
				}

				return;
			}

			Logger.Error($"Unknown command: `{tokens[0]}`.");
		}

		/// <summary>
		///		Does the command exist in the command list?
		/// </summary>
		/// <param name="command"></param>
		/// <returns>Returns <c>true</c> if the command exists</returns>
		[PublicAPI]
		public static bool DoesCommandExist([NotNull] string command)
		{
			if(string.IsNullOrWhiteSpace(command))
				throw new ArgumentNullException(nameof(command));

			return commands.ContainsKey(command);
		}

		[ConsoleCommand("help", "Gets a list of all commands")]
		// ReSharper disable once UnusedMember.Local
#pragma warning disable IDE0060 // Remove unused parameter
		private static void HelpCommand(string[] args)
#pragma warning restore IDE0060 // Remove unused parameter
		{
			foreach ((string command, CommandInfo commandInfo) in commands)
			{
				Logger.Info("`{@Command}` - {@Summary}", command, commandInfo.CommandSummary);
			}
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
			while (pos < input.Length && " \t".IndexOf(input[pos]) > -1)
			{
				pos++;
			}
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