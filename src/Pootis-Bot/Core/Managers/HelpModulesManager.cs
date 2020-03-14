using System.Collections.Generic;
using System.Linq;
using Pootis_Bot.Core.Logging;
using Pootis_Bot.Entities;
using Pootis_Bot.Modules.Account;
using Pootis_Bot.Modules.Audio;
using Pootis_Bot.Modules.Basic;
using Pootis_Bot.Modules.Fun;
using Pootis_Bot.Modules.Steam;

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
		/// Checks all the help modules in the config
		/// </summary>
		public static void CheckHelpModules()
		{
			foreach (string module in _helpModules.SelectMany(helpModule =>
				helpModule.Modules.Where(module => DiscordModuleManager.GetModule(module) == null)))
				Logger.Log(
					$"There is no module called {module}! Reset the help modules or fix the help modules in the config file!",
					LogVerbosity.Error);
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
				Modules = new List<string> {nameof(Basic), nameof(Misc)}
			};
			helpModules.Add(basic);

			HelpModule utils = new HelpModule
			{
				Group = "Utils",
				Modules = new List<string> {nameof(Utils)}
			};
			helpModules.Add(utils);

			HelpModule voting = new HelpModule
			{
				Group = "Voting",
				Modules = new List<string> {nameof(Voting)}
			};
			helpModules.Add(voting);

			HelpModule account = new HelpModule
			{
				Group = "Account",
				Modules = new List<string> {nameof(AccountDataManagement), nameof(AccountUtils)}
			};
			helpModules.Add(account);

			HelpModule fun = new HelpModule
			{
				Group = "Fun",
				Modules = new List<string>
				{
					nameof(GiphySearch), nameof(GoogleSearch), nameof(YoutubeSearch), nameof(WikipediaSearch),
					nameof(RandomPerson), nameof(TronaldDump), nameof(SteamUserUtils)
				}
			};
			helpModules.Add(fun);

			HelpModule audio = new HelpModule
			{
				Group = "Audio",
				Modules = new List<string> {nameof(Music)}
			};
			helpModules.Add(audio);

			return helpModules;
		}
	}
}