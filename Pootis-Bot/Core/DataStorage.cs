using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Pootis_Bot.Entities;

namespace Pootis_Bot.Core
{
	public static class DataStorage
	{
		#region User Accounts

		/// <summary>
		/// Saves all user accounts
		/// </summary>
		/// <param name="accounts">A list of all the user accounts to save</param>
		/// <param name="filePath">Where to save the file</param>
		public static void SaveUserAccounts(IEnumerable<UserAccount> accounts, string filePath)
		{
			string json = JsonConvert.SerializeObject(accounts, Config.bot.ResourceFilesFormatting);
			File.WriteAllText(filePath, json);
		}

		/// <summary>
		/// Loads all the user accounts
		/// </summary>
		/// <param name="filePath"></param>
		/// <returns></returns>
		public static IEnumerable<UserAccount> LoadUserAccounts(string filePath)
		{
			if (!File.Exists(filePath)) return null;
			string json = File.ReadAllText(filePath);
			return JsonConvert.DeserializeObject<List<UserAccount>>(json);
		}

		#endregion

		#region Server List

		/// <summary>
		/// Saves a list of servers
		/// </summary>
		/// <param name="serverLists">A list of servers to save</param>
		/// <param name="filePath">Where to save to</param>
		public static void SaveServerList(IEnumerable<ServerList> serverLists, string filePath)
		{
			string json = JsonConvert.SerializeObject(serverLists, Config.bot.ResourceFilesFormatting);
			File.WriteAllText(filePath, json);
		}

		/// <summary>
		/// Loads a list of all the servers from file
		/// </summary>
		/// <param name="filePath">The path to the server list json file</param>
		/// <returns></returns>
		public static IEnumerable<ServerList> LoadServerList(string filePath)
		{
			if (!File.Exists(filePath)) return null;
			string json = File.ReadAllText(filePath);
			return JsonConvert.DeserializeObject<List<ServerList>>(json);
		}

		#endregion

		#region Help Modules

		/// <summary>
		/// Saves a list of help modules
		/// </summary>
		/// <param name="helpModules"></param>
		/// <param name="filePath"></param>
		public static void SaveHelpModules(IEnumerable<HelpModule> helpModules, string filePath)
		{
			string json = JsonConvert.SerializeObject(helpModules, Config.bot.ResourceFilesFormatting);
			File.WriteAllText(filePath, json);
		}

		/// <summary>
		/// Loads a list of all the help modules from a file
		/// </summary>
		/// <param name="filePath"></param>
		/// <returns></returns>
		public static IEnumerable<HelpModule> LoadHelpModules(string filePath)
		{
			if (!File.Exists(filePath)) return null;
			string json = File.ReadAllText(filePath);
			return JsonConvert.DeserializeObject<List<HelpModule>>(json);
		}

		#endregion

		/// <summary>
		/// Checks if a saved user file exists
		/// </summary>
		/// <param name="filePath">The path to the user save file</param>
		/// <returns></returns>
		public static bool SaveExists(string filePath)
		{
			return File.Exists(filePath);
		}
	}
}