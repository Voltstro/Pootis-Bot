using System.Collections.Generic;
using System.Linq;
using Discord.Commands;

namespace Pootis_Bot.Core
{
	public class DiscordModuleManager
	{
		private static CommandService _commands;

		public static void SetupModuleManager(CommandService commandService)
		{
			_commands = commandService;
		}

		/// <summary>
		/// Get a module
		/// </summary>
		/// <param name="moduleName"></param>
		/// <returns></returns>
		public static ModuleInfo GetModule(string moduleName)
		{
			IEnumerable<ModuleInfo> result = from a in _commands.Modules
				where a.Name == moduleName
				select a;

			ModuleInfo module = result.FirstOrDefault();
			return module;
		}
	}
}
