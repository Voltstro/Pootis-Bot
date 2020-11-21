using System;

namespace Pootis_Bot.Console
{
	/// <summary>
	///     A command that can be executed in the console
	/// </summary>
	[AttributeUsage(AttributeTargets.Method)]
	public class ConsoleCommand : Attribute
	{
		internal readonly string Command;

		internal readonly string CommandSummary;

		/// <summary>
		///     Creates a new <see cref="ConsoleCommand" />
		/// </summary>
		/// <param name="command">What command to enter into the console</param>
		/// <param name="summary">A basic summary of the command</param>
		public ConsoleCommand(string command, string summary)
		{
			Command = command;
			CommandSummary = summary;
		}
	}
}