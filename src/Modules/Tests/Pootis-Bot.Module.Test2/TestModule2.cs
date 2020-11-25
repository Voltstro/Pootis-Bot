using System;
using Pootis_Bot.Modules;

namespace Pootis_Bot.Module.Test2
{
	public class TestModule2 : Modules.Module
	{
		public override ModuleInfo GetModuleInfo()
		{
			return new ModuleInfo("TestModule2", new Version(1, 0));
		}
	}
}