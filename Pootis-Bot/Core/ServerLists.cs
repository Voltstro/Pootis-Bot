using Discord.WebSocket;
using Pootis_Bot.Entities;
using System.Collections.Generic;
using System.Linq;

namespace Pootis_Bot.Core
{
    public class ServerLists
    {
        internal static List<GlobalServerList> serverLists;

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
                WelcomeMessageEnabled = false,
                WelcomeChannel = 0,
                WelcomeGoodbyeMessage = "Goodbye [user]. We hope you enjoyed your stay.",
                WelcomeMessage = "Hello [user]! Thanks for joining [server]. Please check out the rules first then enjoy your stay.",
                RuleEnabled = false,
                RuleRole = null,
                RuleMessageID = 0
            };

            serverLists.Add(newServer);
            SaveServerList();
            return newServer;
        }
    }
}
