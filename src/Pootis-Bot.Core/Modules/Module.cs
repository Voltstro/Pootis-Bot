using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Discord.WebSocket;
using Pootis_Bot.Commands.Permissions;

#nullable enable
namespace Pootis_Bot.Modules
{
#pragma warning disable 1998
	/// <summary>
	///     A module for Pootis-Bot. Can be used to add command and functions to the bot
	/// </summary>
	public abstract class Module
	{
		private ModuleInfo cachedModuleInfo;

		/// <summary>
		///     Gets info relating to the modules
		/// </summary>
		/// <returns></returns>
		public abstract ModuleInfo GetModuleInfo();

		/// <summary>
		///     Called on initialization
		/// </summary>

		public virtual async Task Init()
		{
		}

		/// <summary>
		///     Called after all modules are initialized.
		///     <para>
		///         Here is a good spot to check if other modules are loaded
		///         with <see cref="ModuleManager.CheckIfModuleIsLoaded" />, in-case you want to soft-depend on another module.
		///     </para>
		/// </summary>
		public virtual async Task PostInit()
		{
		}

		/// <summary>
		///		Called when the <see cref="DiscordSocketClient"/> connects
		/// </summary>
		/// <param name="client"></param>
		public virtual async Task ClientConnected([DisallowNull] DiscordSocketClient client)
		{
		}

		/// <summary>
		///     Called on shutdown
		/// </summary>
		public virtual void Shutdown()
		{
		}

		/// <summary>
		///		Return a non-null <see cref="IPermissionProvider"/> to add a permission provider to Pootis's command handler.
		/// </summary>
		/// <returns></returns>
		public virtual IPermissionProvider? AddPermissionProvider()
		{
			return null;
		}

		/// <summary>
		///		Call this if you are accessing <see cref="ModuleInfo"/> from Pootis's core
		///		<para>It uses a cached version of <see cref="ModuleInfo"/>, just in-case the module returns something different each time.</para>
		/// </summary>
		/// <returns></returns>
		internal ModuleInfo GetModuleInfoInternal()
		{
			if (cachedModuleInfo.ModuleName == null)
				cachedModuleInfo = GetModuleInfo();

			return cachedModuleInfo;
		}
	}
#pragma warning restore 1998
}