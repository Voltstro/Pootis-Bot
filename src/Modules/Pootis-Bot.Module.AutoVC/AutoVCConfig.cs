using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Pootis_Bot.Config;

namespace Pootis_Bot.Module.AutoVC
{
    internal class AutoVCConfig : Config<AutoVCConfig>
    {
        public string BaseName { get; set; } = "➕ New {0} VC";
        
        public List<AutoVC> AutoVCs { get; set; } = new();

        [SkipLocalsInit]
        public bool TryGetAutoVC(ulong channelId, out AutoVC autoVC)
        {
            Unsafe.SkipInit(out autoVC);

            AutoVC foundAutoVc = AutoVCs.Find(vc => vc.ChannelId == channelId);
            if (foundAutoVc == null)
                return false;

            autoVC = foundAutoVc;
            return true;
        }

        public void AddAutoVc(ulong channelId, ulong guildId, string name)
        {
            AutoVCs.Add(new AutoVC
            {
                ChannelId = channelId,
                GuildId = guildId,
                ChannelName = name,
                ActiveSubAutoVc = new List<ulong>()
            });
        }
    }
}