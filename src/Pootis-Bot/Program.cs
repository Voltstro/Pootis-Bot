using System;
using System.Collections.Generic;
using Pootis_Bot.PackageDownloader;

namespace Pootis_Bot
{
	public static class Program
	{
		public static void Main(string[] args)
		{
			List<string> result = new NuGetPackageResolver("net5.0").DownloadPackage("Discord.Net", new Version(2, 2, 0)).GetAwaiter().GetResult();
			foreach (string dll in result)
			{
				Console.WriteLine(dll);
			}

			Console.ReadKey();
		}
	}
}