using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Pootis_Bot.Config;

namespace Pootis_Bot.Module.AutoVC;

/// <summary>
///     Config for auto VC module
/// </summary>
public class AutoVCConfig : Config<AutoVCConfig>
{
    /// <summary>
    ///     The base name the auto VCs will have
    /// </summary>
    public string BaseName { get; set; } = "➕ New {0} VC";

    /// <summary>
    ///     List of auto VCs
    /// </summary>
    public List<AutoVC> AutoVCs { get; set; } = new();

    /// <summary>
    ///     Trys to get an auto VC
    /// </summary>
    /// <param name="channelId"></param>
    /// <param name="autoVC"></param>
    /// <returns></returns>
    [SkipLocalsInit]
    public bool TryGetAutoVC(ulong channelId, out AutoVC autoVC)
    {
        Unsafe.SkipInit(out autoVC);

        AutoVC? foundAutoVc = AutoVCs.Find(vc => vc.ChannelId == channelId);
        if (foundAutoVc == null)
            return false;

        autoVC = foundAutoVc;
        return true;
    }

    /// <summary>
    ///     Adds an auto VC
    /// </summary>
    /// <param name="channelId"></param>
    /// <param name="guildId"></param>
    /// <param name="name"></param>
    public void AddAutoVc(ulong channelId, ulong guildId, string name)
    {
        AutoVCs.Add(new AutoVC(channelId, guildId, name));
    }
}