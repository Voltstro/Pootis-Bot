using System.Collections.Generic;

namespace Pootis_Bot.Module.AutoVC;

public class AutoVC
{
    public AutoVC(ulong guildId, ulong channelId, string channelName)
    {
        GuildId = guildId;
        ChannelId = channelId;
        ChannelName = channelName;
        ActiveSubAutoVc = new List<ulong>();
    }

    public ulong GuildId { get; }

    public ulong ChannelId { get; }

    public string ChannelName { get; }

    public List<ulong> ActiveSubAutoVc { get; }
}