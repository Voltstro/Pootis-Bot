using System;
using Pootis_Bot.Modules;

namespace Pootis_Bot.Module.Test2
{
	public class TestModule2 : IModule
	{
		public void Dispose()
		{
		}

		public ModuleInfo GetModuleInfo()
		{
			return new ModuleInfo("TestModule2", new Version(1, 0));
		}

		public void Init()
		{
			
		}
	}
}