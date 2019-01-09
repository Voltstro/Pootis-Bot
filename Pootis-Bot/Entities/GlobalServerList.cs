using Pootis_Bot.Entities.Server;
using System.Collections.Generic;
using System.Linq;

namespace Pootis_Bot.Entities
{
    public class GlobalServerList
    {
        public ulong serverID;

        public bool enableWelcome;

        public ulong welcomeID;

        public bool isRules;

        public string staffRoleName;

        public string adminRoleName;

        //Misc. command permissions
        public string permEmbedMessage;

        //Profile mang. command permissions
        public string permNotWarnableRole;
        public string permMakeWarnableRole;
        public string permWarn;
        public string permYT;
        public string permGiphy;
        public string permGoogle;

        public List<GlobalServerBanedChannelList> banedChannels = new List<GlobalServerBanedChannelList>();

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
