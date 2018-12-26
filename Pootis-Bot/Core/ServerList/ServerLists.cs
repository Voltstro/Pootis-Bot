using Discord.WebSocket;
using System.Collections.Generic;
using System.Linq;

namespace Pootis_Bot.Core.ServerList
{
    public class ServerLists
    {
        private static List<ServerList> serverLists;

        private readonly static string serverListFile = "Resources/serverlist.json";

        static ServerLists()
        {
            if (DataStorage.SaveExisits(serverListFile))
            {
                serverLists = DataStorage.LoadServerList(serverListFile).ToList();
            }
            else
            {
                serverLists = new List<ServerList>();
                SaveServerList();
            }
        }

        public static void SaveServerList()
        {
            DataStorage.SaveServerList(serverLists, serverListFile);
        }

        public static ServerList GetServer(SocketGuild server)
        {
            return GetOrCreateServer(server.Id);
        }

        private static ServerList GetOrCreateServer(ulong id)
        {
            var result = from a in serverLists
                         where a.serverID == id
                         select a;

            var server = result.FirstOrDefault();
            if (server == null) server = CreateServer(id);
            return server;
        }

        private static ServerList CreateServer(ulong id)
        {
            var newServer = new ServerList()
            {
                serverID = id,
                enableWelcome = false,
                welcomeID = 0,
                adminRoleName = "Admin"
            };

            serverLists.Add(newServer);
            SaveServerList();
            return newServer;
        }
    }
}
