using System;
using JetBrains.Annotations;

namespace Pootis_Bot.Modules
{
	/// <summary>
	///     A NuGet package required by a module
	/// </summary>
	public struct ModuleNuGetPackage
	{
		/// <summary>
		///     Creates a new <see cref="ModuleNuGetPackage" />
		/// </summary>
		/// <param name="packageId">The package id. (Whats it name on NuGet)</param>
		/// <param name="packageVersion">The version of the package</param>
		/// <param name="assemblyName">The name of the assembly that the package will extract to</param>
		public ModuleNuGetPackage([NotNull] string packageId, [NotNull] Version packageVersion, [NotNull] string assemblyName)
		{
			//Null check
			if(string.IsNullOrWhiteSpace(packageId))
				throw new ArgumentNullException(nameof(packageId));

			if(string.IsNullOrWhiteSpace(assemblyName))
				throw new ArgumentNullException(nameof(assemblyName));

			PackageId = packageId;
			PackageVersion = packageVersion ?? throw new ArgumentNullException(nameof(packageVersion));
			AssemblyName = assemblyName;
		}

		/// <summary>
		///     The package id. (Whats it name on NuGet)
		/// </summary>
		[NotNull] public readonly string PackageId;

		/// <summary>
		///     The package version
		/// </summary>
		[NotNull] public readonly Version PackageVersion;

		/// <summary>
		///     The name of the assembly that the package will extract to
		/// </summary>
		[NotNull] public readonly string AssemblyName;
	}
}