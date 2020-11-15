using System;

namespace Pootis_Bot.Modules
{
	/// <summary>
	/// Info for a module
	/// </summary>
	public struct ModuleInfo
	{
		/// <summary>
		/// Creates a new module info instance
		/// </summary>
		/// <param name="name">The name of the module</param>
		/// <param name="version">The version of the version</param>
		public ModuleInfo(string name, Version version)
		{
			ModuleName = name;
			ModuleVersion = version;
		}

		/// <summary>
		/// The name of the module
		/// </summary>
		public readonly string ModuleName;

		/// <summary>
		/// The name of the module
		/// </summary>
		public readonly Version ModuleVersion;
	}
}