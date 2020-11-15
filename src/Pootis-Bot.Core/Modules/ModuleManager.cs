using System;
using System.Linq;
using System.Reflection;
using Pootis_Bot.Core.Logging;

namespace Pootis_Bot.Modules
{
	public sealed class ModuleManager
	{
		/// <summary>
		/// Loads all modules
		/// </summary>
		public void LoadModules(Assembly assembly)
		{
			foreach (Type type in assembly.GetTypes().Where(x => x.IsClass && x.IsPublic))
			{
				if (!typeof(IModule).IsAssignableFrom(type)) continue;

				if (!(Activator.CreateInstance(type) is IModule module)) continue;

				ModuleInfo moduleInfo = module.GetModuleInfo();

				Logger.Info("Loaded module {@Module} version {@Version}", moduleInfo.ModuleName, moduleInfo.ModuleVersion.ToString());
			}
		}
	}
}