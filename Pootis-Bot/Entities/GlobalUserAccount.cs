using System;
using System.Collections.Generic;
using System.Linq;

namespace Pootis_Bot.Entities
{
    public class GlobalUserAccount
    {
        public ulong ID { get; set; }

        public uint Points { get; set; }

        public uint XP { get; set; }

        public string Msg { get; set; }

        public uint LevelNumber
        {
            get
            {
                return (uint)Math.Sqrt(XP / 30);
            }
        }

        public List<GlobalUserAccountServer> servers = new List<GlobalUserAccountServer>();

        public class GlobalUserAccountServer
        {
            public ulong ServerID { get; set; }
            public int Warnings { get; set; }
            public bool IsAccountNotWarnable { get; set; }
        }

        public GlobalUserAccountServer GetOrCreateServer(ulong id)
        {
            var result = from a in servers
                         where a.ServerID == id
                         select a;

            var server = result.FirstOrDefault();
            if (server == null) server = CreateServer(id);
            return server;
        }

        GlobalUserAccountServer CreateServer(ulong _serverID)
        {
            var serveritem = new GlobalUserAccountServer
            {
                ServerID = _serverID,
                IsAccountNotWarnable = false,
                Warnings = 0
            };

            servers.Add(serveritem);
            return serveritem;
        }

    }
}
