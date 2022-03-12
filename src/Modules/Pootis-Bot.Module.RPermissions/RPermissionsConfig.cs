using System.Collections.Generic;
using System.Linq;
using Pootis_Bot.Config;
using Pootis_Bot.Module.RPermissions.Entities;

namespace Pootis_Bot.Module.RPermissions;

/// <summary>
///     Config for RPermissions
/// </summary>
public class RPermissionsConfig : Config<RPermissionsConfig>
{
    public List<RPermissionServer> Servers { get; } = new();

    /// <summary>
    ///     Does a guild exist?
    /// </summary>
    /// <param name="guildId"></param>
    /// <returns></returns>
    public bool DoesServerExist(ulong guildId)
    {
        return Servers.Exists(x => x.GuildId == guildId);
    }

    /// <summary>
    ///     Get or create a <see cref="RPermissionServer"/>
    /// </summary>
    /// <param name="guildId"></param>
    /// <returns></returns>
    public RPermissionServer GetOrCreateServer(ulong guildId)
    {
        RPermissionServer server = Servers.FirstOrDefault(x => x.GuildId == guildId) ?? CreateServer(guildId);
        return server;
    }
    
    private RPermissionServer CreateServer(ulong guildId)
    {
        RPermissionServer server = new(guildId);
        Servers.Add(server);
        Save();
        return server;
    }
}