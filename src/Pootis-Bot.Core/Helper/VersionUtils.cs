using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Pootis_Bot.Helper
{
	/// <summary>
	///		Utils for getting version numbers
	/// </summary>
	public static class VersionUtils
	{
		/// <summary>
		///		Gets the version of the application
		/// </summary>
		/// <returns></returns>
		public static string GetApplicationVersion()
		{
			Assembly assembly = Assembly.GetEntryAssembly();
			return assembly == null
				? null
				: assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()
					?.InformationalVersion ?? string.Empty;
		}

		/// <summary>
		///		Gets the calling assembly version
		/// </summary>
		/// <returns></returns>
		public static string GetCallingVersion()
		{
			Assembly assembly = Assembly.GetCallingAssembly();
			AssemblyInformationalVersionAttribute assemblyVersion = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
			return assemblyVersion?.InformationalVersion;
		}

		/// <summary>
		///		Gets the version of Discord.Net
		///		<para>Please note that Discord.Net needs to be loaded before this will work</para>
		/// </summary>
		/// <returns></returns>
		/// <exception cref="NullReferenceException"></exception>
		public static string GetDiscordNetVersion()
		{
			Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

			Assembly discordCore = assemblies.FirstOrDefault(x => x.GetName().Name == "Discord.Net.Core");
			if(discordCore == null)
				throw new NullReferenceException("Discord.Net.Core doesn't appear to be loaded!");

			return discordCore.GetCustomAttribute<AssemblyInformationalVersionAttribute>()
				?.InformationalVersion ?? string.Empty;
		}
	}
}