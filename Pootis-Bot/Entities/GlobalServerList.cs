using System.Collections.Generic;
using System.Linq;

namespace Pootis_Bot.Entities
{
    public class GlobalServerList
    {
        public ulong ServerID { get; set; }

        public bool EnableWelcome { get; set; }

        public ulong WelcomeID { get; set; }

        public bool IsRules { get; set; }

        public string StaffRoleName { get; set; }

        public string AdminRoleName { get; set; }

        public Permissions permissions;

        public List<GlobalServerBanedChannelList> banedChannels = new List<GlobalServerBanedChannelList>();

        public class Permissions
        {
            public string PermEmbedMessage { get; set; }
            public string PermNotWarnableRole { get; set; }
            public string PermMakeWarnableRole { get; set; }
            public string PermWarn { get; set; }
            public string PermYT { get; set; }
            public string PermGiphy { get; set; }
            public string PermGoogle { get; set; }
        }

        public class GlobalServerBanedChannelList
        {
            public ulong channelID;
        }

        public GlobalServerBanedChannelList GetOrCreateBanedChannel(ulong id)
        {
            var result = from a in banedChannels
                         where a.channelID == id
                         select a;

            var channel = result.FirstOrDefault();
            if (channel == null) channel = CreateBanedChannel(id);
            return channel;
        }

        GlobalServerBanedChannelList CreateBanedChannel(ulong _channelID)
        {
            var banedchannelitem = new GlobalServerBanedChannelList
            {
                channelID = _channelID
            };

            banedChannels.Add(banedchannelitem);
            return banedchannelitem;
        }

        public GlobalServerBanedChannelList[] GetAllBanedChannels()
        {
            GlobalServerBanedChannelList[] convert = banedChannels.ToArray();
            return convert;
        }

        public void DeleteChannel(ulong id)
        {
            var channel = GetOrCreateBanedChannel(id);
            banedChannels.Remove(channel);         
        }

    }
}
