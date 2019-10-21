using System.Collections.Generic;
using System.Linq;
using Discord.WebSocket;
using Pootis_Bot.Entities;
using Pootis_Bot.Structs;

namespace Pootis_Bot.Core
{
	public class ServerListsManager
	{
		private const string ServerListFile = "Resources/serverlist.json";
		public static List<ServerList> Servers;

		static ServerListsManager()
		{
			if (DataStorage.SaveExists(ServerListFile))
			{
				Servers = DataStorage.LoadServerList(ServerListFile).ToList();
			}
			else
			{
				Servers = new List<ServerList>();
				SaveServerList();
			}
		}

		/// <summary>
		/// Saves all the servers
		/// </summary>
		public static void SaveServerList()
		{
			DataStorage.SaveServerList(Servers, ServerListFile);
		}

		/// <summary>
		/// Gets a server, or creates one if needed
		/// </summary>
		/// <param name="server"></param>
		/// <returns></returns>
		public static ServerList GetServer(SocketGuild server)
		{
			return GetOrCreateServer(server.Id);
		}

		private static ServerList GetOrCreateServer(ulong id)
		{
			IEnumerable<ServerList> result = from a in Servers
				where a.GuildId == id
				select a;

			ServerList server = result.FirstOrDefault();
			if (server == null) server = CreateServer(id);
			return server;
		}

		private static ServerList CreateServer(ulong id)
		{
			ServerList newServer = new ServerList
			{
				GuildId = id,
				PointGiveAmount = 10,
				PointsGiveCooldownTime = 15,
				BannedChannels = new List<ulong>(),
				ServerRolePoints = new List<ServerRolePoints>(),
				CommandInfos = new List<ServerList.CommandInfo>(),
				RoleToRoleMentions = new List<RoleToRoleMention>(),
				AutoVoiceChannels = new List<VoiceChannel>(),
				RoleGives = new List<RoleGive>(),
				ActiveAutoVoiceChannels = new List<ulong>(),
				WelcomeMessageEnabled = false,
				WelcomeChannelId = 0,
				WelcomeGoodbyeMessage = "Goodbye [user]. We hope you enjoyed your stay.",
				WelcomeMessage =
					"Hello [user]! Thanks for joining **[server]**. Please check out the rules first then enjoy your stay.",
				RuleEnabled = false,
				RuleRoleId = 0,
				RuleMessageId = 0,
				AntiSpamSettings = new ServerList.AntiSpamSettingsInfo
				{
					RoleToRoleMentionWarnings = 3, MentionUsersPercentage = 45, MentionUserEnabled = true
				}
			};

			Servers.Add(newServer);
			SaveServerList();
			return newServer;
		}
	}
}