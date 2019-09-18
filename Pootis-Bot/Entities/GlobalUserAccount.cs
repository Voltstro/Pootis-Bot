using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Pootis_Bot.Entities
{
    public class GlobalUserAccount
    {
        public ulong Id { get; set; }

        public uint Xp { get; set; }

        public string Msg { get; set; }

        [JsonIgnore]
        public uint LevelNumber => (uint)Math.Sqrt(Xp / 30);

        public List<GlobalUserAccountServer> Servers = new List<GlobalUserAccountServer>();

        public class GlobalUserAccountServer
        {
            public ulong ServerId { get; set; }
            public int Warnings { get; set; }
            public bool IsAccountNotWarnable { get; set; }

            [JsonIgnore]
            public DateTime LastLevelUpTime { get; set; }

			[JsonIgnore]
			public int RoleToRoleMentionWarnings { get; set; }
        }

        public GlobalUserAccountServer GetOrCreateServer(ulong id)
        {
            var result = from a in Servers
                         where a.ServerId == id
                         select a;

            var server = result.FirstOrDefault();
            if (server == null) server = CreateServer(id);
            return server;
        }

        private GlobalUserAccountServer CreateServer(ulong serverId)
        {
	        var serverItem = new GlobalUserAccountServer
	        {
		        ServerId = serverId,
		        IsAccountNotWarnable = false,
		        Warnings = 0
	        };

	        Servers.Add(serverItem);
	        return serverItem;
        }
    }
}
