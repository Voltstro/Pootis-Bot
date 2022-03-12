using System;
using Pootis_Bot.Commands.Permissions;
using Pootis_Bot.Helper;
using Pootis_Bot.Modules;

namespace Pootis_Bot.Module.RPermissions;

internal sealed class RPermissionsModule : Modules.Module
{
    protected override ModuleInfo GetModuleInfo()
    {
        return new ModuleInfo("RPermissionsModule", "Voltstro", Version.Parse(VersionUtils.GetCallingVersion()));
    }

    protected override IPermissionProvider AddPermissionProvider()
    {
        return new RPermissionsProvider();
    }
}