namespace Pootis_Bot.ConsoleCommandHandler
{
	public class ConsoleCommand
	{
		public ConsoleCommand(string name, Method method)
		{
			CommandName = name;
			Method = method;
		}

		/// <summary>
		/// The command, it self
		/// </summary>
		public string CommandName { get; set; }

		/// <summary>
		/// The <see cref="ConsoleCommandHandler.Method"/> to execute when the command is run
		/// </summary>
		public Method Method { get; set; }
	}
}