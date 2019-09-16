using System.Collections.Generic;
using System.Linq;
using Discord.WebSocket;
using Pootis_Bot.Entities;

namespace Pootis_Bot.Core
{
    public class ServerLists
    {
        public static List<GlobalServerList> Servers;

        private const string ServerListFile = "Resources/serverlist.json";

        static ServerLists()
        {
            if (DataStorage.SaveExists(ServerListFile))
            {
                Servers = DataStorage.LoadServerList(ServerListFile).ToList();
            }
            else
            {
                Servers = new List<GlobalServerList>();
                SaveServerList();
            }
        }

        public static void SaveServerList()
        {
            DataStorage.SaveServerList(Servers, ServerListFile);
        }

        public static GlobalServerList GetServer(SocketGuild server)
        {
            return GetOrCreateServer(server.Id);
        }

        private static GlobalServerList GetOrCreateServer(ulong id)
        {
            var result = from a in Servers
                         where a.ServerId == id
                         select a;

            var server = result.FirstOrDefault();
            if (server == null) server = CreateServer(id);
            return server;
        }

        private static GlobalServerList CreateServer(ulong id)
        {
            var newServer = new GlobalServerList()
            {
                ServerId = id,
                WelcomeMessageEnabled = false,
                WelcomeChannel = 0,
                WelcomeGoodbyeMessage = "Goodbye [user]. We hope you enjoyed your stay.",
                WelcomeMessage = "Hello [user]! Thanks for joining **[server]**. Please check out the rules first then enjoy your stay.",
                RuleEnabled = false,
                RuleRole = null,
                RuleMessageId = 0
            };

			newServer.AntiSpamSettings = new GlobalServerList.AntiSpamSettingsInfo
			{
				RoleToRoleMentionWarnings = 3,
				MentionUsersPercentage = 45,
				MentionUserEnabled = true
			};

            Servers.Add(newServer);
            SaveServerList();
            return newServer;
        }
    }
}
