using System;
using System.Threading.Tasks;
using Discord.WebSocket;
using Pootis_Bot.Helper;
using Pootis_Bot.Modules;

namespace Pootis_Bot.Module.Basic;

public sealed class BasicModule : Modules.Module
{
    protected override ModuleInfo GetModuleInfo()
    {
        return new ModuleInfo("BasicModule", "Voltstro", new Version(VersionUtils.GetCallingVersion()));
    }

    protected override Task ClientConnected(DiscordSocketClient client)
    {
        GameStatusManager.OnConnected(client);
        return Task.CompletedTask;
    }
}