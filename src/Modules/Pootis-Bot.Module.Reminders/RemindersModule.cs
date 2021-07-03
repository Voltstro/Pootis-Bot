using System;
using System.Threading.Tasks;
using Discord.WebSocket;
using Pootis_Bot.Helper;
using Pootis_Bot.Modules;

namespace Pootis_Bot.Module.Reminders
{
    internal sealed class RemindersModule : Modules.Module
    {
        protected override ModuleInfo GetModuleInfo()
        {
            return new ModuleInfo("RemindersModule", "Voltstro", new Version(VersionUtils.GetCallingVersion()));
        }

        protected override Task ClientReady(DiscordSocketClient client, bool firstReady)
        {
            if (firstReady)
                RemindersService.StartAllReminders(client);
            return base.ClientReady(client, firstReady);
        }
    }
}