namespace Pootis_Bot.ConsoleCommandHandler
{
	public class ConsoleCommand
	{
		public ConsoleCommand(string name, string summary, Method method)
		{
			CommandName = name;
			CommandSummary = summary;
			Method = method;
		}

		/// <summary>
		/// The command, it self
		/// </summary>
		public string CommandName { get; set; }

		/// <summary>
		/// A quick summary for this command
		/// </summary>
		public string CommandSummary { get; set; }

		/// <summary>
		/// The <see cref="ConsoleCommandHandler.Method"/> to execute when the command is run
		/// </summary>
		public Method Method { get; set; }
	}
}