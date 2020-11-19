using System;
using JetBrains.Annotations;

namespace Pootis_Bot.Modules
{
	/// <summary>
	///     A NuGet package required by a module
	/// </summary>
	public struct ModuleDependency
	{
		/// <summary>
		///     Sets-up a dependency for a NuGet package
		/// </summary>
		/// <param name="packageId">The package id. (Whats it name on NuGet)</param>
		/// <param name="packageVersion">The version of the package</param>
		/// <param name="assemblyName">The name of the assembly that the package will extract to</param>
		[PublicAPI]
		public ModuleDependency([NotNull] string packageId, [NotNull] Version packageVersion,
			[NotNull] string assemblyName)
		{
			//Null check
			if (string.IsNullOrWhiteSpace(packageId))
				throw new ArgumentNullException(nameof(packageId));

			if (string.IsNullOrWhiteSpace(assemblyName))
				throw new ArgumentNullException(nameof(assemblyName));

			PackageId = packageId;
			PackageVersion = packageVersion ?? throw new ArgumentNullException(nameof(packageVersion));
			AssemblyName = assemblyName;
			ModuleName = null;
		}

		/// <summary>
		///		Sets-up a dependency for another module
		/// </summary>
		/// <param name="moduleName">The name of the module to depend on</param>
		[PublicAPI]
		public ModuleDependency([NotNull] string moduleName)
		{
			if(string.IsNullOrWhiteSpace(moduleName))
				throw new ArgumentNullException(nameof(moduleName));

			PackageId = null;
			PackageVersion = null;
			AssemblyName = null;
			ModuleName = moduleName;
		}

		/// <summary>
		///     The package id. (Whats it name on NuGet)
		/// </summary>
		internal readonly string PackageId;

		/// <summary>
		///     The package version
		/// </summary>
		internal readonly Version PackageVersion;

		/// <summary>
		///     The name of the assembly that the package will extract to
		/// </summary>
		internal readonly string AssemblyName;

		/// <summary>
		///		The module that a module depends on
		/// </summary>
		internal readonly string ModuleName;
	}
}