using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Discord;
using Discord.WebSocket;

namespace Pootis_Bot.Core
{
	/// <summary>
	/// Contains some general config and methods.
	/// </summary>
	public static class Global
	{
		//Here is a list of sites that Pootis-Bot can refer to, if your hosting your own version of the documentation you might wanna change it.
		public static readonly string githubPage = "https://github.com/Creepysin/Pootis-Bot"; //Main github page

		public static readonly string websiteHome = "https://pootis-bot.creepysin.com"; //Main docs page

		public static readonly string
			websiteCommands =
				"https://pootis-bot.creepysin.com/commands/discord-commands/"; //Main Discord commands list

		public static readonly string
			websiteServerSetup = "https://pootis-bot.creepysin.com/server-setup/"; //Main server-setup page

		public static readonly string websiteConsoleCommands =
			"https://pootis-bot.creepysin.com/commands/console-commands/";

		//An array of Discord servers, add as many as you want and use it throughout the bot.
		//                                                  Main Server                -  Development Server
		public static readonly string[] discordServers =
			{"https://discord.creepysin.com", "https://discord.gg/m4YcsUa"};

		public static readonly string version = "0.3.4";

		public static readonly string aboutMessage = $"Pootis Bot --- | --- {version}\n" +
		                                             $"Created by Creepysin licensed under the MIT license. Visit {githubPage}/blob/master/LICENSE.md for more info.\n\n" +
		                                             "Pootis Robot icon by Valve\n" +
		                                             "Created with Discord.NET\n" +
		                                             "https://github.com/Creepysin/Pootis-Bot \n\n" +
		                                             "Thank you for using Pootis Bot";

		public static string BotName;
		public static string BotPrefix;
		public static string BotToken;

		/// <summary>
		/// Logs a message to the console
		/// </summary>
		/// <param name="msg">The message? Yea the message</param>
		/// <param name="color">The color of the message</param>
		public static void Log(string msg, ConsoleColor color)
		{
			Console.ForegroundColor = color;
			Console.WriteLine($"[{TimeNow()}] " + msg);
			Console.ForegroundColor = ConsoleColor.White;
		}

		/// <summary>
		/// Logs a message to the console
		/// </summary>
		/// <param name="msg"></param>
		public static void Log(string msg)
		{
			Console.WriteLine($"[{TimeNow()}] " + msg);
		}

		/// <summary>
		/// Gets... you guessed it, THE TIME NOW!!!! (12hr time)
		/// </summary>
		/// <returns></returns>
		public static string TimeNow()
		{
			return DateTime.Now.ToString("h:mm:ss tt");
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
		/// Checks if a given role exists in a guild
		/// </summary>
		/// <param name="guild"></param>
		/// <param name="rolename"></param>
		/// <returns></returns>
		public static IRole CheckIfRoleExist(SocketGuild guild, string rolename)
		{
			IEnumerable<SocketRole> result = from a in guild.Roles
				where a.Name == rolename
				select a;

			SocketRole role = result.FirstOrDefault();
			return role;
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