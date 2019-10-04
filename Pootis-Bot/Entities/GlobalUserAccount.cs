using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Pootis_Bot.Entities
{
	public class GlobalUserAccount
	{
		public List<GlobalUserAccountServer> Servers = new List<GlobalUserAccountServer>();
		public ulong Id { get; set; }

		public uint Xp { get; set; }

		public string ProfileMsg { get; set; }

		/// <summary>
		/// What level is the user on?
		/// </summary>
		[JsonIgnore] public uint LevelNumber => (uint) Math.Sqrt(Xp / 30f);

		/// <summary>
		/// Gets or creates a server from an ID
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		public GlobalUserAccountServer GetOrCreateServer(ulong id)
		{
			IEnumerable<GlobalUserAccountServer> result = from a in Servers
				where a.ServerId == id
				select a;

			GlobalUserAccountServer server = result.FirstOrDefault() ?? CreateServer(id);
			return server;
		}

		private GlobalUserAccountServer CreateServer(ulong serverId)
		{
			GlobalUserAccountServer serverItem = new GlobalUserAccountServer
			{
				ServerId = serverId,
				IsAccountNotWarnable = false,
				Warnings = 0
			};

			Servers.Add(serverItem);
			return serverItem;
		}

		public class GlobalUserAccountServer
		{
			public ulong ServerId { get; set; }
			public int Warnings { get; set; }
			public bool IsAccountNotWarnable { get; set; }

			public bool IsMuted { get; set; }

			[JsonIgnore] public DateTime LastLevelUpTime { get; set; }

			[JsonIgnore] public int RoleToRoleMentionWarnings { get; set; }
		}
	}
}