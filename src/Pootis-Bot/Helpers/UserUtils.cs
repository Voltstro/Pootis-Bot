using System.Linq;
using Discord;
using Discord.WebSocket;

namespace Pootis_Bot.Helpers
{
	/// <summary>
	/// Provides methods for user related functions
	/// </summary>
	public static class UserUtils
	{
		/// <summary>
		/// Kicks a user
		/// </summary>
		/// <param name="user">The user who is getting kicked</param>
		/// <param name="kicker">The user who is kicking user</param>
		/// <param name="message">The message that will be included</param>
		/// <returns></returns>
		public static void KickUser(this SocketGuildUser user, SocketUser kicker, string message)
		{
			user.KickAsync($"{message} | Kicked by {kicker.Username}").GetAwaiter().GetResult();
		}

		/// <summary>
		/// Bans a user
		/// </summary>
		/// <param name="user">The user who is getting banned</param>
		/// <param name="kicker">The user who is banning user</param>
		/// <param name="message">The message that will be included</param>
		/// <returns></returns>
		public static void BanUser(this SocketGuildUser user, SocketUser kicker, string message)
		{
			user.BanAsync(0, $"{message} | Banned by {kicker.Username}").GetAwaiter().GetResult();
		}

		/// <summary>
		/// Checks to see if the user has a role
		/// </summary>
		/// <param name="user">The user to check</param>
		/// <param name="roleId">The ID of the role that we checking against</param>
		/// <returns></returns>
		public static bool UserHaveRole(this SocketGuildUser user, ulong roleId)
		{
			IRole role = user.Roles.FirstOrDefault(x => x.Id == roleId);
			return role != null;
		}
	}
}