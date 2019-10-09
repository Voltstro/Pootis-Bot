using System;
using Newtonsoft.Json;

namespace Pootis_Bot.Entities
{
	public class UserAccountServerData
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
