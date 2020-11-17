using System;

namespace Pootis_Bot.Modules
{
	/// <summary>
	///     A module for Pootis-Bot. Can be used to add command and functions to the bot
	/// </summary>
	public interface IModule : IDisposable
	{
		/// <summary>
		///     Gets info relating to the modules
		/// </summary>
		/// <returns></returns>
		public ModuleInfo GetModuleInfo();

		/// <summary>
		///     Initializes the module
		/// </summary>
		public void Init();
	}
}