using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Pootis_Bot.Core;
using Pootis_Bot.Logging;
using Pootis_Bot.PackageDownloader;

namespace Pootis_Bot.Modules
{
	/// <summary>
	///     Handles the loading of modules
	/// </summary>
	internal sealed class ModuleManager : IDisposable
	{
		private readonly string assembliesDirectory;
		private readonly ModuleLoadContext loadContext;

		private readonly List<IModule> modules;

		private readonly string modulesDirectory;

		/// <summary>
		///     Creates a new module manager instance
		/// </summary>
		/// <param name="modulesDir">The directory where the modules are kept</param>
		/// <param name="assembliesDir">The directory of where external assemblies exist</param>
		public ModuleManager([NotNull] string modulesDir, [NotNull] string assembliesDir)
		{
			if (string.IsNullOrWhiteSpace(modulesDir))
				throw new ArgumentNullException(nameof(modulesDir));

			if (string.IsNullOrWhiteSpace(assembliesDir))
				throw new ArgumentNullException(nameof(assembliesDir));

			modulesDirectory = $"{Bot.ApplicationLocation}/{modulesDir}";
			assembliesDirectory = $"{Bot.ApplicationLocation}/{assembliesDir}";
			modules = new List<IModule>();
			loadContext = new ModuleLoadContext(modulesDirectory, assembliesDirectory);
		}

		/// <summary>
		///     Disposes of this <see cref="ModuleManager" /> instance
		/// </summary>
		public void Dispose()
		{
			foreach (IModule module in modules)
			{
				Logger.Info("Shutting down module {@ModuleName}...", module.GetModuleInfo().ModuleName);
				module.Dispose();
			}
		}

		/// <summary>
		///     Loads all modules in the <see cref="modulesDirectory" />
		/// </summary>
		internal void LoadModules()
		{
			//Make sure the modules directory exists
			if (!Directory.Exists(modulesDirectory))
				Directory.CreateDirectory(modulesDirectory);

			//Pre-create package resolver
			NuGetPackageResolver packageResolver =
				new NuGetPackageResolver("netstandard2.1", $"{Bot.ApplicationLocation}/Packages/");

			//Get all dlls in the directory
			string[] dlls = Directory.GetFiles(modulesDirectory, "*.dll");
			List<IModule> modulesToInit = new List<IModule>();
			foreach (string dll in dlls)
			{
				Assembly loadedAssembly = LoadModule(dll);
				modulesToInit.AddRange(LoadModulesInAssembly(loadedAssembly));
			}

			//Verify its dependencies
			VerifyModuleDependencies(ref modulesToInit, packageResolver);
			packageResolver.Dispose();

			//Init all the modules
			for (int i = 0; i < modulesToInit.Count; i++)
			{
				ModuleInfo moduleInfo = modulesToInit[i].GetModuleInfo();

				//Call the init function
				try
				{
					modulesToInit[i].Init();
					Logger.Info("Loaded module {@Module} version {@Version}", moduleInfo.ModuleName,
						moduleInfo.ModuleVersion.ToString());
					modules.Add(modulesToInit[i]);
				}
				catch (Exception ex)
				{
					Logger.Error(
						"Something when wrong while initializing {@ModuleName}! The module will not be loaded. Ex: {@Exception}",
						moduleInfo.ModuleName, ex.Message);
					modulesToInit.RemoveAt(i);
				}
			}
		}

		/// <summary>
		///		Checks if a module is loaded
		/// </summary>
		/// <param name="moduleName"></param>
		/// <returns></returns>
		[PublicAPI]
		public bool CheckIfModuleIsLoaded(string moduleName)
		{
			return modules.Exists(x => x.GetModuleInfo().ModuleName == moduleName);
		}

		private Assembly LoadModule(string dllPath)
		{
			return loadContext.LoadFromAssemblyPath(dllPath);
		}

		private IEnumerable<IModule> LoadModulesInAssembly(Assembly assembly)
		{
			List<IModule> foundModules = new List<IModule>();

			foreach (Type type in assembly.GetTypes().Where(x => x.IsClass && x.IsPublic))
			{
				if (!typeof(IModule).IsAssignableFrom(type)) continue;

				IModule module;
				try
				{
					module = Activator.CreateInstance(type) as IModule;
				}
				catch (MissingMethodException ex)
				{
					Logger.Error(
						"Something when wrong while creating a module from the assembly {@AssemblyName}! It looks like the constructor could be weird! The module will not be loaded: Ex: {@Exception}",
						assembly.FullName, ex.Message);
					continue;
				}

				if (module == null)
					continue;

				//Our first contact with the module code it self, get info about it
				try
				{
					module.GetModuleInfo();
				}
				catch (Exception ex)
				{
					Logger.Error(
						"Something when wrong while trying to obtain module info from the assembly {@AssemblyName}! The module will not be loaded: Ex: {@Exception}",
						assembly.FullName, ex.Message);
					continue;
				}

				//Add the module to the list
				foundModules.Add(module);
			}

			return foundModules;
		}

		private void VerifyModuleDependencies(ref List<IModule> modulesToVerify, NuGetPackageResolver resolver)
		{
			for (int i = 0; i < modulesToVerify.Count; i++)
			{
				ModuleInfo info = modulesToVerify[i].GetModuleInfo();

				//Resolve NuGet packages
				VerifyModuleNuGetPackages(info.Dependencies.Where(x => x.PackageId != null), info, resolver);

				foreach (ModuleDependency moduleDependency in info.Dependencies)
				{
					//Determine if it is a NuGet package or module dependency
					if (moduleDependency.PackageId != null) continue;

					//The module doesn't exist
					if (modulesToVerify.Exists(x => x.GetModuleInfo().ModuleName == moduleDependency.ModuleName)) continue;

					Logger.Error("The module {@Module} depends on the module {@Dependent} which has not been loaded!", info.ModuleName, moduleDependency.ModuleName);
					modulesToVerify.RemoveAt(i);
				}
			}
		}

		private void VerifyModuleNuGetPackages(IEnumerable<ModuleDependency> nugetDependencies, ModuleInfo moduleInfo, NuGetPackageResolver packageResolver)
		{
			if (!Directory.Exists(assembliesDirectory))
				Directory.CreateDirectory(assembliesDirectory);

			foreach (ModuleDependency nuGetPackage in nugetDependencies)
			{
				//The assembly already exists, it should be safe to assume that other dlls that it requires exist as well
				if (File.Exists($"{assembliesDirectory}/{nuGetPackage.AssemblyName}.dll")) continue;

				Logger.Info("Restoring NuGet packages for {@ModuleName}...", moduleInfo.ModuleName);

				//Download the packages and extract them
				List<string> dlls = packageResolver.DownloadPackage(nuGetPackage.PackageId, nuGetPackage.PackageVersion)
					.GetAwaiter()
					.GetResult();

				Logger.Info("Extracting NuGet packages...");

				foreach (string dll in dlls)
				{
					string dllName = Path.GetFileName(dll);
					string destination = $"{assembliesDirectory}/{dllName}";

					//The dll already exists, we don't need to copy it again
					if (File.Exists(destination))
						continue;

					//The required dll exists in the root
					if (File.Exists($"{Bot.ApplicationLocation}/{dllName}"))
						continue;

					File.Copy(dll, destination, true);
				}

				Logger.Info("Packages for {@ModuleName} restored.", moduleInfo.ModuleName);
			}
		}
	}
}