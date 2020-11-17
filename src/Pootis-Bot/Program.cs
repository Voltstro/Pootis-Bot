using System;
using Pootis_Bot;
using Pootis_Bot.Core;

namespace Pootis_Bot
{
	public static class Program
	{
		public static void Main(string[] args)
		{
			Bot bot = new Bot();
			bot.Run();

			Console.ReadKey();

			bot.Dispose();
		}
	}
}