using System;
using System.IO;
using Pootis_Bot.Console;
using Pootis_Bot.Helper;
using Pootis_Bot.Logging;
using Pootis_Bot.Modules;

namespace Pootis_Bot.Module.Upgrade
{
    internal sealed class UpgradeModule : Modules.Module
    {
        protected override ModuleInfo GetModuleInfo()
        {
            return new ModuleInfo("UpgradeModule", "Voltstro", new Version(VersionUtils.GetCallingVersion()));
        }

        [ConsoleCommand("upgrade", "Upgrades 1.x Pootis-Bot config to 2.0 format")]
        public static void Upgrade()
        {
            Logger.Info("This command will upgrade 1.x Pootis-Bot config files to 2.x. Enter in the path of the previous config files location:");
            string folderLocation = System.Console.ReadLine();
            if (!Directory.Exists(folderLocation) || !File.Exists($"{folderLocation}/Config.json"))
            {
                Logger.Error("Invalid folder location!");
                return;
            }
            
            UpgradeService.UpgradeConfigFiles(folderLocation);
            Logger.Info("Upgrade completed! Restart to apply everything.");
        }
    }
}