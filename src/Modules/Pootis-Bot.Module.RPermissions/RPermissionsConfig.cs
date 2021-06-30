using System.Collections.Generic;
using System.Linq;
using Pootis_Bot.Config;
using Pootis_Bot.Module.RPermissions.Entities;

namespace Pootis_Bot.Module.RPermissions
{
    internal class RPermissionsConfig : Config<RPermissionsConfig>
    {
        public List<RPermissionServer> Servers { get; set; } = new List<RPermissionServer>();

        public bool DoesServerExist(ulong guildId)
        {
            return Servers.Exists(x => x.GuildId == guildId);
        }
        
        public RPermissionServer GetOrCreateServer(ulong guildId)
        {
            RPermissionServer server = Servers.FirstOrDefault(x => x.GuildId == guildId) ?? CreateServer(guildId);
            return server;
        }

        private RPermissionServer CreateServer(ulong guildId)
        {
            RPermissionServer server = new RPermissionServer
            {
                GuildId = guildId
            };
            Servers.Add(server);
            Save();
            return server;
        }
    }
}