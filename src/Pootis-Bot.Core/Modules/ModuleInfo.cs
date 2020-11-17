using System;
using JetBrains.Annotations;

namespace Pootis_Bot.Modules
{
	/// <summary>
	///     Info for a module
	/// </summary>
	public struct ModuleInfo
	{
		/// <summary>
		///     Creates a new module info instance
		/// </summary>
		/// <param name="name">The name of the module</param>
		/// <param name="version">The version of the version</param>
		/// <param name="packages">Packages required by the module</param>
		public ModuleInfo([NotNull] string name, [NotNull] Version version, params ModuleNuGetPackage[] packages)
		{
			if(string.IsNullOrWhiteSpace(name))
				throw new ArgumentNullException(nameof(name));

			ModuleName = name;
			ModuleVersion = version ?? throw new ArgumentNullException(nameof(version));
			NuGetPackages = packages;
		}

		/// <summary>
		///     Creates a new module info instance
		/// </summary>
		/// <param name="name">The name of the module</param>
		/// <param name="version">The version of the version</param>
		public ModuleInfo([NotNull] string name, [NotNull] Version version)
		{
			if(string.IsNullOrWhiteSpace(name))
				throw new ArgumentNullException(nameof(name));

			ModuleName = name;
			ModuleVersion = version ?? throw new ArgumentNullException(nameof(version));
			NuGetPackages = Array.Empty<ModuleNuGetPackage>();
		}

		/// <summary>
		///     The name of the module
		/// </summary>
		[NotNull] public readonly string ModuleName;

		/// <summary>
		///     The name of the module
		/// </summary>
		[NotNull] public readonly Version ModuleVersion;

		/// <summary>
		///     NuGet packages used by the module
		/// </summary>
		[NotNull] public readonly ModuleNuGetPackage[] NuGetPackages;
	}
}