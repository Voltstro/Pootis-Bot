using System;
using System.Threading.Tasks;
using Discord.WebSocket;
using Pootis_Bot.Helper;
using Pootis_Bot.Modules;

namespace Pootis_Bot.Module.Fun;

public sealed class FunModule : Modules.Module
{
    protected override ModuleInfo GetModuleInfo()
    {
        return new ModuleInfo("FunModule", "Voltstro", new Version(VersionUtils.GetCallingVersion()), new ModuleDependency("Wiki.Net", new Version(3, 1, 0), "Wiki.Net.dll"));
    }

    protected override Task ClientConnected(DiscordSocketClient client)
    {
        return Task.CompletedTask;
    }
}