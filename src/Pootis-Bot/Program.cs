using Pootis_Bot.Core;

namespace Pootis_Bot
{
	public static class Program
	{
		public static void Main(string[] args)
		{
			Bot bot = new Bot();
			bot.Run();
			bot.ConsoleLoop();

			bot.Dispose();
		}
	}
}