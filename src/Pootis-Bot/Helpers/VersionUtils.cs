using System;
using System.Linq;
using System.Reflection;

namespace Pootis_Bot.Helpers
{
	public class VersionUtils
	{
		/// <summary>
		/// Gets the app version
		/// </summary>
		/// <returns></returns>
		public static string GetAppVersion()
		{
			Assembly assembly = Assembly.GetEntryAssembly();
			if (assembly == null) return null;

			return assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()
				.InformationalVersion;
		}

		/// <summary>
		/// Gets <see cref="Discord"/>'s <see cref="AssemblyInformationalVersionAttribute"/> and returns it as a <see cref="string"/>
		/// </summary>
		/// <returns></returns>
		public static string GetDiscordNetVersion()
		{
			Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

			return (from assembly in assemblies where assembly.GetName().Name == "Discord.Net.Core" select assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion).FirstOrDefault();
		}
	}
}