using Discord.WebSocket;
using System.Collections.Generic;
using System.Linq;

namespace Pootis_Bot.Entities
{
    public class GlobalServerList
    {
        public ulong ServerID { get; set; }

        public ulong WelcomeChannel { get; set; }
        public bool WelcomeMessageEnabled { get; set; }
        public string WelcomeMessage { get; set; }
        public string WelcomeGoodbyeMessage { get; set; }

        public bool RuleEnabled { get; set; }
        public ulong RuleMessageID { get; set; }
        public string RuleRole { get; set; }
        public string RuleReactionEmoji { get; set; }

        public List<GlobalServerBanedChannelList> banedChannels = new List<GlobalServerBanedChannelList>();

        public List<CommandInfo> commandInfos = new List<CommandInfo>();

        public class CommandInfo
        {
            public string Command { get; set; }
            public List<string> Roles = new List<string>();
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

        public CommandInfo GetCommandInfo(string command)
        {
            var result = from a in commandInfos
                         where a.Command == command
                         select a;

            var commandInfo = result.FirstOrDefault();
            return commandInfo;
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