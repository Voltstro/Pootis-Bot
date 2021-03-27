using System;
using Pootis_Bot.Modules;

namespace Pootis_Bot.Module.Profiles
{
	public sealed class ProfilesModule : Modules.Module
	{
		public override ModuleInfo GetModuleInfo()
		{
			return new ModuleInfo("ProfilesModule", "Voltstro", new Version(1, 0, 0));
		}
	}
}