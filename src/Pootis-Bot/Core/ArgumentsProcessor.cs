using System;
using System.CommandLine;
using System.Linq;
using Pootis_Bot.Core.Logging;

namespace Pootis_Bot.Core
{
	/// <summary>
	/// Process arguments
	/// </summary>
	public static class ArgumentsProcessor
	{
		public static string AudioLibsApiUrl;

		public static void ParseArguments(string[] args)
		{
			RootCommand rootCommand = new RootCommand
			{
				new Option<string>("-resdir", 
					getDefaultValue: () => "Resources/",
					description: "The directory to where the resource files are"),

				new Option<string>("-audiolibsurl", 
					getDefaultValue: () => "https://pootis-bot.voltstro.dev/download/externallibfiles.json", 
					description: "The URL to where information about audio libs will be received")
			};

			rootCommand.Description = "Pootis-Bot";
			rootCommand.Handler = System.CommandLine.Invocation.CommandHandler.Create<string, string>(
				(resDir, audioLibsUrl) =>
				{
					Global.ResourcesDirectory = resDir;
					AudioLibsApiUrl = audioLibsUrl;
				});

			rootCommand.Invoke(args);

			//There might be a better way of doing this using System.CommandLine, but I really can't find one at the current time using the current API
			//If there is one, open up a PR to fix it if you want to
			if (args.Contains("-h") || args.Contains("/h") || args.Contains("--help") || args.Contains("-?") || args.Contains("/h") || args.Contains("--version"))
			{
				//Close the app if help or version was in the args
				Logger.Shutdown();
				Environment.Exit(0);
			}
		}
	}
}