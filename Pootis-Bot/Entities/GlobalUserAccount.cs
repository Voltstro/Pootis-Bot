using System.Collections.Generic;
using System.Linq;

namespace Pootis_Bot.Entities
{
    public class GlobalUserAccount
    {
        public ulong ID { get; set; }

        public uint Points { get; set; }

        public uint XP { get; set; }

        public List<GlobalUserAccountServer> servers = new List<GlobalUserAccountServer>();


        public GlobalUserAccountServer GetOrCreateServer(ulong id)
        {
            var result = from a in servers
                         where a.serverID == id
                         select a;

            var server = result.FirstOrDefault();
            if (server == null) server = CreateServer(id);
            return server;
        }

        GlobalUserAccountServer CreateServer(ulong _serverID)
        {
            var serveritem = new GlobalUserAccountServer
            {
                serverID = _serverID,
                isNotWarnable = false,
                warnings = 0
            };

            servers.Add(serveritem);
            return serveritem;
        }

    }
}
