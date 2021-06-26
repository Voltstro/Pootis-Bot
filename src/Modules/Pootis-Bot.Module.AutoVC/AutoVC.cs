using System.Collections.Generic;

namespace Pootis_Bot.Module.AutoVC
{
    internal class AutoVC
    {
        public ulong GuildId { get; set; }
        
        public ulong ChannelId { get; set; }
        
        public string ChannelName { get; set; }

        public List<ulong> ActiveSubAutoVc { get; set; }
    }
}