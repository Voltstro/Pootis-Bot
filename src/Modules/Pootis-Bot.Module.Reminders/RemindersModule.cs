using System;
using System.Threading.Tasks;
using Discord.WebSocket;
using Pootis_Bot.Helper;
using Pootis_Bot.Modules;

namespace Pootis_Bot.Module.Reminders
{
    public class RemindersModule : Modules.Module
    {
        private DiscordSocketClient discordClient;
        
        public override ModuleInfo GetModuleInfo()
        {
            return new ModuleInfo("RemindersModule", "Voltstro", new Version(VersionUtils.GetCallingVersion()));
        }

        public override Task ClientConnected(DiscordSocketClient client)
        {
            discordClient = client;
            client.Ready += ClientReady;

            return base.ClientConnected(client);
        }

        private Task ClientReady()
        {
            discordClient.Ready -= ClientReady;
            RemindersService.StartAllReminders(discordClient);
            return Task.CompletedTask;
        }
    }
}