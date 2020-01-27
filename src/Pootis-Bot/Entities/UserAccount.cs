using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Newtonsoft.Json;

namespace Pootis_Bot.Entities
{
	public class UserAccount
	{
		/// <summary>
		/// The ID of the user
		/// </summary>
		public ulong Id { get; set; }

		/// <summary>
		/// How much XP does this user have? Xp/Level number are across servers and are NOT server specific
		/// </summary>
		public uint Xp { get; set; }

		/// <summary>
		/// What message does the user have set for their profile
		/// </summary>
		[DefaultValue("")]
		public string ProfileMsg { get; set; }

		/// <summary>
		/// A list of all the user's <see cref="UserAccountServerData"/>
		/// </summary>
		public List<UserAccountServerData> Servers { get; set; }

		/// <summary>
		/// What level is the user on?
		/// </summary>
		[JsonIgnore]
		public uint LevelNumber => (uint) Math.Sqrt(Xp / 30f);

		/// <summary>
		/// Gets or creates a <see cref="UserAccountServerData"/>
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		public UserAccountServerData GetOrCreateServer(ulong id)
		{
			IEnumerable<UserAccountServerData> result = from a in Servers
				where a.ServerId == id
				select a;

			UserAccountServerData serverData = result.FirstOrDefault() ?? CreateServer(id);
			return serverData;
		}

		private UserAccountServerData CreateServer(ulong serverId)
		{
			UserAccountServerData serverDataItem = new UserAccountServerData
			{
				ServerId = serverId,
				IsAccountNotWarnable = false,
				Warnings = 0
			};

			Servers.Add(serverDataItem);
			return serverDataItem;
		}
	}
}