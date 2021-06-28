using System;
using System.Threading.Tasks;
using Discord.WebSocket;
using Pootis_Bot.Helper;
using Pootis_Bot.Modules;

namespace Pootis_Bot.Module.WelcomeMessage
{
    public class WelcomeMessageModule : Modules.Module
    {
        public override ModuleInfo GetModuleInfo()
        {
            return new ModuleInfo("WelcomeMessageModule", "Voltstro", new Version(VersionUtils.GetCallingVersion()));
        }

        public override Task ClientConnected(DiscordSocketClient client)
        {
            client.Ready += () =>
            {
                _ = Task.Run(() => WelcomeMessageService.CheckAllServers(client));
                return Task.CompletedTask;
            };
            client.UserJoined += WelcomeMessageService.UserJoined;
            client.UserLeft += WelcomeMessageService.UserLeft;
            client.ChannelDestroyed += WelcomeMessageService.ChannelDeleted;
            
            return base.ClientConnected(client);
        }
    }
}