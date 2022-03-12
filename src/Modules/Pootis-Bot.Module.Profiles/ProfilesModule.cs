using System;
using System.Threading.Tasks;
using Discord.WebSocket;
using Pootis_Bot.Helper;
using Pootis_Bot.Modules;

namespace Pootis_Bot.Module.Profiles;

internal sealed class ProfilesModule : Modules.Module
{
    //It won't be null
    private XpLevelManager xpLevelManager = null!;

    protected override ModuleInfo GetModuleInfo()
    {
        return new ModuleInfo("ProfilesModule", "Voltstro", new Version(VersionUtils.GetCallingVersion()));
    }

    protected override Task Init()
    {
        xpLevelManager = new XpLevelManager();
        return base.Init();
    }

    protected override async Task ClientMessage(DiscordSocketClient client, SocketUserMessage message)
    {
        await xpLevelManager.HandelUserMessage(message);
    }
}