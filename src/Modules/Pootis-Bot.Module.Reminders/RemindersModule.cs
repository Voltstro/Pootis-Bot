using System;
using Pootis_Bot.Helper;
using Pootis_Bot.Modules;

namespace Pootis_Bot.Module.Reminders
{
    public class RemindersModule : Modules.Module
    {
        public override ModuleInfo GetModuleInfo()
        {
            return new ModuleInfo("RemindersModule", "Voltstro", new Version(VersionUtils.GetCallingVersion()));
        }
    }
}