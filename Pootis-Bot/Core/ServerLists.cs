using Discord.WebSocket;
using Pootis_Bot.Entities;
using System.Collections.Generic;
using System.Linq;

namespace Pootis_Bot.Core
{
    public class ServerLists
    {
        private static List<GlobalServerList> serverLists;

        private readonly static string serverListFile = "Resources/serverlist.json";

        static ServerLists()
        {
            if (DataStorage.SaveExisits(serverListFile))
            {
                serverLists = DataStorage.LoadServerList(serverListFile).ToList();
            }
            else
            {
                serverLists = new List<GlobalServerList>();
                SaveServerList();
            }
        }

        public static void SaveServerList()
        {
            DataStorage.SaveServerList(serverLists, serverListFile);
        }

        public static GlobalServerList GetServer(SocketGuild server)
        {
            return GetOrCreateServer(server.Id);
        }

        private static GlobalServerList GetOrCreateServer(ulong id)
        {
            var result = from a in serverLists
                         where a.ServerID == id
                         select a;

            var server = result.FirstOrDefault();
            if (server == null) server = CreateServer(id);
            return server;
        }

        private static GlobalServerList CreateServer(ulong id)
        {
            var newServer = new GlobalServerList()
            {
                ServerID = id,
                EnableWelcome = false,
                WelcomeID = 0,
                AdminRoleName = "Admin"
            };

            newServer.permissions = new GlobalServerList.Permissions();

            serverLists.Add(newServer);
            SaveServerList();
            return newServer;
        }
    }
}
