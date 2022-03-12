using System;
using System.Threading.Tasks;
using Discord.WebSocket;
using Pootis_Bot.Helper;
using Pootis_Bot.Modules;

namespace Pootis_Bot.Module.WelcomeMessage;

internal sealed class WelcomeMessageModule : Modules.Module
{
    protected override ModuleInfo GetModuleInfo()
    {
        return new ModuleInfo("WelcomeMessageModule", "Voltstro", new Version(VersionUtils.GetCallingVersion()));
    }

    protected override Task ClientConnected(DiscordSocketClient client)
    {
        client.UserJoined += WelcomeMessageService.UserJoined;
        client.UserLeft += WelcomeMessageService.UserLeft;
        client.ChannelDestroyed += WelcomeMessageService.ChannelDeleted;

        return base.ClientConnected(client);
    }

    protected override Task ClientReady(DiscordSocketClient client, bool firstReady)
    {
        _ = Task.Run(() => WelcomeMessageService.CheckAllServers(client));
        return base.ClientReady(client, firstReady);
    }
}