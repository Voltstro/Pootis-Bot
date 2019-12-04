using System.Collections.Generic;
using System.Linq;
using Discord.WebSocket;

namespace Pootis_Bot.Helpers
{
	public class RoleUtils
	{
		/// <summary>
		/// Gets a role in a guild
		/// </summary>
		/// <param name="guild"></param>
		/// <param name="roleName"></param>
		/// <returns></returns>
		public static SocketRole GetGuildRole(SocketGuild guild, string roleName)
		{
			IEnumerable<SocketRole> result = from a in guild.Roles
				where a.Name == roleName
				select a;

			SocketRole role = result.FirstOrDefault();
			return role;
		}

		/// <summary>
		/// Gets a role in a guild
		/// </summary>
		/// <param name="guild"></param>
		/// <param name="roleId"></param>
		/// <returns></returns>
		public static SocketRole GetGuildRole(SocketGuild guild, ulong roleId)
		{
			IEnumerable<SocketRole> result = from a in guild.Roles
				where a.Id == roleId
				select a;

			SocketRole role = result.FirstOrDefault();
			return role;
		}
	}
}
