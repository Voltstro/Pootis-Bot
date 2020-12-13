using System;
using Pootis_Bot.Modules;

namespace Pootis_Bot.Module.Basic
{
	public sealed class BasicModule : Modules.Module
	{
		public override ModuleInfo GetModuleInfo()
		{
			return new ModuleInfo("BasicModule", "Voltstro", new Version(1, 0, 0));
		}
	}
}