using System;

namespace Pootis_Bot.Modules
{
	public interface IModule : IDisposable
	{
		public ModuleInfo GetModuleInfo();
			
		public void Init();
	}
}