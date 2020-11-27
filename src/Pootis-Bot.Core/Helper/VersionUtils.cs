using System;
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
		public static Version GetApplicationVersion()
		{
			Assembly assembly = Assembly.GetEntryAssembly();
			return assembly == null ? null : new Version(assembly?.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? string.Empty);
		}

		/// <summary>
		///		Gets the version of Discord.Net
		///		<para>Please note that Discord.Net needs to be loaded before this will work</para>
		/// </summary>
		/// <returns></returns>
		/// <exception cref="NullReferenceException"></exception>
		public static Version GetDiscordNetVersion()
		{
			Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

			Assembly discordCore = assemblies.FirstOrDefault(x => x.GetName().Name == "Discord.Net.Core");
			if(discordCore == null)
				throw new NullReferenceException("Discord.Net.Core doesn't appear to be loaded!");

			return new Version(discordCore?.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? string.Empty);
		}
	}
}