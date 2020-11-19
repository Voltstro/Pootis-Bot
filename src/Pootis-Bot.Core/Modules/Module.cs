using System;
using JetBrains.Annotations;

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
		[PublicAPI]
		public virtual void Init()
		{
		}

		/// <summary>
		///		Called on shutdown
		/// </summary>
		[PublicAPI]
		public virtual void Shutdown()
		{
		}
	}
}