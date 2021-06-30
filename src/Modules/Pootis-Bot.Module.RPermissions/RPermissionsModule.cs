using System;
using Pootis_Bot.Commands.Permissions;
using Pootis_Bot.Helper;
using Pootis_Bot.Modules;

namespace Pootis_Bot.Module.RPermissions
{
    public class RPermissionsModule : Modules.Module
    {
        public override ModuleInfo GetModuleInfo()
        {
            return new ModuleInfo("RPermissionsModule", "Voltstro", Version.Parse(VersionUtils.GetCallingVersion()));
        }

        public override IPermissionProvider AddPermissionProvider()
        {
            return new RPermissionsProvider();
        }
    }
}