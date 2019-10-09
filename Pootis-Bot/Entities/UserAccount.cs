using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Pootis_Bot.Entities
{
	public class UserAccount
	{
		/// <summary>
		/// The id of the user
		/// </summary>
		public ulong Id { get; set; }

		/// <summary>
		/// How much XP does this user have? Xp/Level number are across servers and are NOT server specific 
		/// </summary>
		public uint Xp { get; set; }

		/// <summary>
		/// What message does the user have set for their profile
		/// </summary>
		public string ProfileMsg { get; set; }

		/// <summary>
		/// Server specific data
		/// </summary>
		public List<GlobalUserAccountServer> Servers { get; set; }

		/// <summary>
		/// What level is the user on?
		/// </summary>
		[JsonIgnore] public uint LevelNumber => (uint) Math.Sqrt(Xp / 30f);

		/// <summary>
		/// Gets or creates a server from the server's id
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
			/// <summary>
			/// What is the ID of the server
			/// </summary>
			public ulong ServerId { get; set; }

			/// <summary>
			/// How many warnings does a user have on this server
			/// </summary>
			public int Warnings { get; set; }

			/// <summary>
			/// Is the account NOT warnable (if true the account cannot be warned)
			/// </summary>
			public bool IsAccountNotWarnable { get; set; }

			/// <summary>
			/// Is the user muted?
			/// </summary>
			public bool IsMuted { get; set; }

			/// <summary>
			/// What was their last level up time?
			/// </summary>
			[JsonIgnore] public DateTime LastLevelUpTime { get; set; }

			/// <summary>
			/// How many warnings has this user got from pinging a role they were not allowed to?
			/// </summary>
			[JsonIgnore] public int RoleToRoleMentionWarnings { get; set; }
		}
	}
}