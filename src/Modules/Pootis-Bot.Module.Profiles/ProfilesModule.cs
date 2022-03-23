using System;
using Microsoft.Extensions.DependencyInjection;
using Pootis_Bot.Helper;
using Pootis_Bot.Modules;

namespace Pootis_Bot.Module.Profiles;

internal sealed class ProfilesModule : Modules.Module
{
    protected override ModuleInfo GetModuleInfo()
    {
        return new ModuleInfo("ProfilesModule", "Voltstro", new Version(VersionUtils.GetCallingVersion()));
    }

    protected override void AddToServices(IServiceCollection services)
    {
        XpLevelManager xpLevelManager = new();
        services.AddSingleton(xpLevelManager);
    }
}