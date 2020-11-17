using System;

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
		public ModuleNuGetPackage(string packageId, Version packageVersion, string assemblyName)
		{
			PackageId = packageId;
			PackageVersion = packageVersion;
			AssemblyName = assemblyName;
		}

		/// <summary>
		///     The package id. (Whats it name on NuGet)
		/// </summary>
		public readonly string PackageId;

		/// <summary>
		///     The package version
		/// </summary>
		public readonly Version PackageVersion;

		/// <summary>
		///     The name of the assembly that the package will extract to
		/// </summary>
		public readonly string AssemblyName;
	}
}