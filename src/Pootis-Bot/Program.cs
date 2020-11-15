using System;
using System.Collections.Generic;
using Pootis_Bot.Core;
using Pootis_Bot.PackageDownloader;

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