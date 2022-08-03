using System;
using Microsoft.Extensions.DependencyInjection;
using Pootis_Bot.Helper;
using Pootis_Bot.Modules;
using WikiDotNet;

namespace Pootis_Bot.Module.Fun;

public sealed class FunModule : Modules.Module
{
    protected override ModuleInfo GetModuleInfo()
    {
        return new ModuleInfo("FunModule", "Voltstro", new Version(VersionUtils.GetCallingVersion()), new ModuleDependency("Wiki.Net", new Version(4, 1, 0), "Wiki.Net.dll"));
    }
    
    protected override void AddToServices(IServiceCollection services)
    {
        services.AddSingleton<WikiSearcher>();
    }
}