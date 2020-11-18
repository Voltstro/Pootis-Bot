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
		public void LoadModules()
		{
			//Make sure the modules directory exists
			if (!Directory.Exists(modulesDirectory))
				Directory.CreateDirectory(modulesDirectory);

			//Pre-create package resolver
			NuGetPackageResolver packageResolver =
				new NuGetPackageResolver("netstandard2.1", $"{Bot.ApplicationLocation}/Packages/");

			//Get all dlls in the directory
			string[] dlls = Directory.GetFiles(modulesDirectory, "*.dll");
			foreach (string dll in dlls)
			{
				Assembly loadedAssembly = LoadModule(dll);
				LoadModulesInAssembly(loadedAssembly, packageResolver);
			}

			packageResolver.Dispose();
		}

		private Assembly LoadModule(string dllPath)
		{
			return loadContext.LoadFromAssemblyPath(dllPath);
		}

		private void LoadModulesInAssembly(Assembly assembly, NuGetPackageResolver resolver)
		{
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
				ModuleInfo moduleInfo;
				try
				{
					moduleInfo = module.GetModuleInfo();
				}
				catch (Exception ex)
				{
					Logger.Error(
						"Something when wrong while trying to obtain module info from the assembly {@AssemblyName}! The module will not be loaded: Ex: {@Exception}",
						assembly.FullName, ex.Message);
					continue;
				}

				//Verify NuGet packages for the module
				try
				{
					VerifyModuleNuGetPackages(moduleInfo, resolver);
				}
				catch (Exception ex)
				{
					Logger.Error(
						"Something when wrong while trying to resolve NuGet packages for {@ModuleName}! The module will not be loaded: Ex: {@Exception}",
						moduleInfo.ModuleName, ex.Message);
					continue;
				}

				//Call the init function
				try
				{
					module.Init();
					Logger.Info("Loaded module {@Module} version {@Version}", moduleInfo.ModuleName,
						moduleInfo.ModuleVersion.ToString());
				}
				catch (Exception ex)
				{
					Logger.Error(
						"Something when wrong while initializing {@ModuleName}! The module will not be loaded. Ex: {@Exception}",
						moduleInfo.ModuleName, ex.Message);
					continue;
				}

				//Add the module to the list so we can call to it later
				modules.Add(module);
			}
		}

		private void VerifyModuleNuGetPackages(ModuleInfo moduleInfo, NuGetPackageResolver packageResolver)
		{
			if (!Directory.Exists(assembliesDirectory))
				Directory.CreateDirectory(assembliesDirectory);

			foreach (ModuleNuGetPackage nuGetPackage in moduleInfo.NuGetPackages)
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