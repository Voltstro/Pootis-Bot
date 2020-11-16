using System;

namespace Pootis_Bot.Modules
{
	public struct ModuleNuGetPackage
	{
		public ModuleNuGetPackage(string packageId, Version packageVersion, string assemblyName)
		{
			PackageId = packageId;
			PackageVersion = packageVersion;
			AssemblyName = assemblyName;
		}

		public string PackageId;

		public Version PackageVersion;

		public string AssemblyName;
	}
}