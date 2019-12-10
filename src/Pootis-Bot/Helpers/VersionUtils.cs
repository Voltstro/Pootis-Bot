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
	}
}