using System.Collections.Generic;
using System.Linq;
using Pootis_Bot.Structs;

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

        public List<ulong> BanedChannels = new List<ulong>();

        public List<CommandInfo> CommandInfos = new List<CommandInfo>();

        public List<VoiceChannel> VoiceChannels = new List<VoiceChannel>();

        public List<ulong> ActiveAutoVoiceChannels = new List<ulong>();

        public class CommandInfo
        {
            public string Command { get; set; }
            public List<string> Roles = new List<string>();
        }

        public ulong GetOrCreateBanedChannel(ulong id)
        {
            var result = from a in BanedChannels
                         where a == id
                         select a;

            var channel = result.FirstOrDefault();
            if (channel == 0) channel = CreateBanedChannel(id);
            return channel;
        }

        public VoiceChannel GetVoiceChannel(ulong id)
        {
            var result = from a in VoiceChannels
                         where a.ID == id
                         select a;

            var channel = result.FirstOrDefault();
            return channel;
        }

        public ulong GetActiveVoiceChannel(ulong id)
        {
            var result = from a in ActiveAutoVoiceChannels
                         where a == id
                         select a;

            var channel = result.FirstOrDefault();
            if (channel == 0) channel = 0;
            return channel;
        }

        public CommandInfo GetCommandInfo(string command)
        {
            var result = from a in CommandInfos
                         where a.Command == command
                         select a;

            var commandInfo = result.FirstOrDefault();
            return commandInfo;
        }

        ulong CreateBanedChannel(ulong _channelID)
        {
            BanedChannels.Add(_channelID);
            return _channelID;
        }
    }
}