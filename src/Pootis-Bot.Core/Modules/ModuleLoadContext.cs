using System.IO;
using System.Reflection;
using System.Runtime.Loader;
using Pootis_Bot.Core;

namespace Pootis_Bot.Modules
{
	internal sealed class ModuleLoadContext : AssemblyLoadContext
	{
		private readonly string assembliesPath;

		public ModuleLoadContext(string assembliesPath)
		{
			this.assembliesPath = assembliesPath;
			Resolving += OnResolving;
		}

		private Assembly OnResolving(AssemblyLoadContext loadContext, AssemblyName assemblyName)
		{
			//Try and load it from the assembly path first
			if (File.Exists($"{assembliesPath}/{assemblyName.Name}.dll"))
				return loadContext.LoadFromAssemblyPath($"{assembliesPath}/{assemblyName.Name}.dll");

			//Try and load it from the root dir
			if (File.Exists($"{Bot.ApplicationLocation}/{assemblyName.Name}.dll"))
				return loadContext.LoadFromAssemblyPath($"{Bot.ApplicationLocation}/{assemblyName.Name}.dll");

			//TODO: NuGet package handling

			return null;
		}
	}
}