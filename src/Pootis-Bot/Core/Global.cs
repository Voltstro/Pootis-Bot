using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using Discord;
using Pootis_Bot.Helpers;

namespace Pootis_Bot.Core
{
	/// <summary>
	/// Contains some general config and methods.
	/// </summary>
	public static class Global
	{
		/// <summary>
		/// The main GitHub page for the project
		/// </summary>
		public static readonly string githubPage = "https://github.com/Creepysin/Pootis-Bot";

		/// <summary>
		/// The main docs page
		/// </summary>
		public static readonly string websiteHome = "https://pootis-bot.creepysin.com";

		/// <summary>
		/// Discord command list
		/// </summary>
		public static readonly string
			websiteCommands =
				"https://pootis-bot.creepysin.com/commands/discord-commands/";

		/// <summary>
		/// The website for setting up the bot on a Discord server
		/// </summary>
		public static readonly string
			websiteServerSetup = "https://pootis-bot.creepysin.com/server-setup/";

		/// <summary>
		/// Console command list
		/// </summary>
		public static readonly string websiteConsoleCommands =
			"https://pootis-bot.creepysin.com/commands/console-commands/";

		/// <summary>
		/// A <see cref="string"/> array of Discord servers, I use to have a 'Developer Creepysin Discord Server', but I shut
		/// it down awhile ago
		/// </summary>
		public static readonly string[] discordServers =
			{"https://discord.creepysin.com"};

		/// <summary>
		/// An about message, for Pootis-Bot
		/// </summary>
		public static readonly string aboutMessage = $"Pootis Bot --- | --- {VersionUtils.GetAppVersion()}\n" +
		                                             $"Created by Creepysin licensed under the MIT license. Visit {githubPage}/blob/master/LICENSE.md for more info.\n\n" +
		                                             "Pootis Robot icon by Valve\n" +
		                                             "Created with Discord.NET\n" +
		                                             "https://github.com/Creepysin/Pootis-Bot \n\n" +
		                                             "Thank you for using Pootis Bot";

		/// <summary>
		/// The bot name
		/// </summary>
		public static string BotName;

		/// <summary>
		/// The bot prefix
		/// </summary>
		public static string BotPrefix;

		/// <summary>
		/// The bot token
		/// </summary>
		public static string BotToken;

		/// <summary>
		/// The bot's current status text
		/// </summary>
		public static string BotStatusText;

		/// <summary>
		/// The bot owner account
		/// </summary>
		public static IUser BotOwner;

		/// <summary>
		/// The bot's logged in account
		/// </summary>
		public static IUser BotUser;

		/// <summary>
		/// Global HTTP client
		/// </summary>
		public static HttpClient HttpClient;

		/// <summary>
		/// Gets... you guessed it, THE TIME NOW!!!! (12hr time)
		/// </summary>
		/// <returns></returns>
		public static string TimeNow()
		{
			return DateTime.Now.ToString("hh:mm:ss tt");
		}

		/// <summary>
		/// Copies a directory from one place to another
		/// </summary>
		/// <param name="sourceDirName"></param>
		/// <param name="destDirName"></param>
		/// <param name="copySubDirs"></param>
		/// <exception cref="DirectoryNotFoundException"></exception>
		public static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
		{
			// Get the subdirectories for the specified directory.
			DirectoryInfo dir = new DirectoryInfo(sourceDirName);

			if (!dir.Exists)
				throw new DirectoryNotFoundException(
					"Source directory does not exist or could not be found: "
					+ sourceDirName);

			DirectoryInfo[] dirs = dir.GetDirectories();
			// If the destination directory doesn't exist, create it.
			if (!Directory.Exists(destDirName)) Directory.CreateDirectory(destDirName);

			// Get the files in the directory and copy them to the new location.
			FileInfo[] files = dir.GetFiles();
			foreach (FileInfo file in files)
			{
				string temppath = Path.Combine(destDirName, file.Name);
				file.CopyTo(temppath, true);
			}

			// If copying subdirectories, copy them and their contents to new location.
			if (copySubDirs)
				foreach (DirectoryInfo subdir in dirs)
				{
					string temppath = Path.Combine(destDirName, subdir.Name);
					DirectoryCopy(subdir.FullName, temppath, true);
				}
		}

		/// <summary>
		/// Turns A String Into A Nicer Looking One
		/// </summary>
		/// <param name="s"></param>
		/// <returns></returns>
		public static string Title(string s)
		{
			char[] array = s.ToCharArray();
			// Handle the first letter in the string.
			if (array.Length >= 1)
				if (char.IsLower(array[0]))
					array[0] = char.ToUpper(array[0]);

			// Scan through the letters, checking for spaces.
			// ... Uppercase the lowercase letters following spaces.
			for (int i = 1; i < array.Length; i++)
				if (array[i - 1] == ' ')
					if (char.IsLower(array[i]))
						array[i] = char.ToUpper(array[i]);

			return new string(array);
		}

		/// <summary>
		/// Gets a random number
		/// </summary>
		/// <param name="min">The minimum</param>
		/// <param name="max">The maximum</param>
		/// <returns></returns>
		public static int RandomNumber(int min, int max)
		{
			Random random = new Random();
			return random.Next(min, max);
		}

		/// <summary>
		/// Checks if a string contains unicode characters
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		public static bool ContainsUnicodeCharacter(string input)
		{
			const int maxAnsiCode = 255;

			return input.Any(c => c > maxAnsiCode);
		}
	}
}