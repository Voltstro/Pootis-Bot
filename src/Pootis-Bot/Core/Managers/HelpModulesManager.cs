using System.Collections.Generic;
using System.Linq;
using Pootis_Bot.Entities;

namespace Pootis_Bot.Core.Managers
{
	public static class HelpModulesManager
	{
		private const string HelpModulesFile = "Resources/HelpModules.json";
		private static List<HelpModule> _helpModules;

		static HelpModulesManager()
		{
			if (DataStorage.SaveExists(HelpModulesFile))
			{
				_helpModules = DataStorage.LoadHelpModules(HelpModulesFile).ToList();
			}
			else
			{
				_helpModules = DefaultHelpModules();
				SaveHelpModules();
			}
		}

		/// <summary>
		/// Saves help modules... obviously
		/// </summary>
		public static void SaveHelpModules()
		{
			DataStorage.SaveHelpModules(_helpModules, HelpModulesFile);
		}

		/// <summary>
		/// Gets all the help modules
		/// </summary>
		/// <returns></returns>
		public static List<HelpModule> GetHelpModules()
		{
			return _helpModules;
		}

		/// <summary>
		/// Resets help modules to their default state
		/// </summary>
		public static void ResetHelpModulesToDefault()
		{
			_helpModules = null;
			_helpModules = DefaultHelpModules();
		}

		private static List<HelpModule> DefaultHelpModules()
		{
			List<HelpModule> helpModules = new List<HelpModule>();

			HelpModule basic = new HelpModule
			{
				Group = "Basic",
				Modules = new List<string> {"BasicCommands", "Misc"}
			};
			helpModules.Add(basic);

			HelpModule utils = new HelpModule
			{
				Group = "Utils",
				Modules = new List<string> {"Utils"}
			};
			helpModules.Add(utils);

			HelpModule account = new HelpModule
			{
				Group = "Account",
				Modules = new List<string> {"AccountDataManagement", "AccountUtils"}
			};
			helpModules.Add(account);

			HelpModule fun = new HelpModule
			{
				Group = "Fun",
				Modules = new List<string>
					{"GiphySearch", "GoogleSearch", "YoutubeSearch", "TronaldDump", "RandomPerson", "WikipediaSearch"}
			};
			helpModules.Add(fun);

			HelpModule audio = new HelpModule
			{
				Group = "Audio",
				Modules = new List<string> {"Music"}
			};
			helpModules.Add(audio);

			return helpModules;
		}
	}
}