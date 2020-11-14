using System;
using System.Collections.Generic;
using Pootis_Bot.PackageDownloader;

namespace Pootis_Bot
{
	public static class Program
	{
		public static void Main(string[] args)
		{
			NuGetPackageResolver resolver = new NuGetPackageResolver("net5.0");
			Console.WriteLine("Discord.Net");
			List<string> discordResult = resolver.DownloadPackage("Discord.Net", new Version(2, 2, 0)).GetAwaiter().GetResult();

			foreach (string dll in discordResult)
			{
				Console.WriteLine(dll);
			}

			Console.WriteLine("Wiki.Net");
			List<string> wikiNetResult = resolver.DownloadPackage("Wiki.Net", new Version(3, 0, 0)).GetAwaiter().GetResult();

			foreach (string dll in wikiNetResult)
			{
				Console.WriteLine(dll);
			}

			Console.ReadKey();
		}
	}
}