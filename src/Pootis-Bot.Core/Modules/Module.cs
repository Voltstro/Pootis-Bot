namespace Pootis_Bot.Modules
{
	/// <summary>
	///     A module for Pootis-Bot. Can be used to add command and functions to the bot
	/// </summary>
	public abstract class Module
	{
		/// <summary>
		///     Gets info relating to the modules
		/// </summary>
		/// <returns></returns>
		public abstract ModuleInfo GetModuleInfo();

		/// <summary>
		///     Called on initialization
		/// </summary>
		public virtual void Init()
		{
		}

		/// <summary>
		///     Called after all modules are initialized.
		///     <para>
		///         Here is a good spot to check if other modules are loaded
		///         with <see cref="ModuleManager.CheckIfModuleIsLoaded" />, in-case you want to soft-depend on another module.
		///     </para>
		/// </summary>
		public virtual void PostInit()
		{
		}

		/// <summary>
		///     Called on shutdown
		/// </summary>
		public virtual void Shutdown()
		{
		}
	}
}