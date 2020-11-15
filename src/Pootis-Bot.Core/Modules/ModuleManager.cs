using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Pootis_Bot.Core.Logging;

namespace Pootis_Bot.Modules
{
	public sealed class ModuleManager : IDisposable
	{
		private readonly List<IModule> modules;

		public ModuleManager()
		{
			modules = new List<IModule>();
		}

		/// <summary>
		/// Loads all modules
		/// </summary>
		public void LoadModulesInAssembly(Assembly assembly)
		{
			bool assemblyCountainsModule = false;

			foreach (Type type in assembly.GetTypes().Where(x => x.IsClass && x.IsPublic))
			{
				if (!typeof(IModule).IsAssignableFrom(type)) continue;

				if (!(Activator.CreateInstance(type) is IModule module)) continue;

				ModuleInfo moduleInfo = module.GetModuleInfo();

				Logger.Info("Loaded module {@Module} version {@Version}", moduleInfo.ModuleName, moduleInfo.ModuleVersion.ToString());

				modules.Add(module);
				assemblyCountainsModule = true;
			}

			if(!assemblyCountainsModule)
				Logger.Error("The assembly {@Assembly} doesn't contain a valid module!", assembly.FullName);
		}

		public void Dispose()
		{
			foreach (IModule module in modules)
			{
				Logger.Info("Shutting down module {@ModuleName}...", module.GetModuleInfo().ModuleName);
				module.Dispose();
			}
		}
	}
}