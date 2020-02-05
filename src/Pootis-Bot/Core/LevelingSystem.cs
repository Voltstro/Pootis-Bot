using Discord.WebSocket;
using Pootis_Bot.Core.Managers;
using Pootis_Bot.Entities;
using Pootis_Bot.Helpers;
using Pootis_Bot.Structs.Server;

namespace Pootis_Bot.Core
{
	public static class LevelingSystem
	{
		/// <summary>
		/// Levels up a user
		/// </summary>
		/// <param name="user"></param>
		/// <param name="channel"></param>
		/// <param name="amount"></param>
		public static async void GiveUserXp(SocketGuildUser user, SocketTextChannel channel, uint amount)
		{
			UserAccount userAccount = UserAccountsManager.GetAccount(user);
			uint oldLevel = userAccount.LevelNumber;

			//Nice one EternalClickbait...

			userAccount.Xp += amount;
			UserAccountsManager.SaveAccounts();

			if (oldLevel != userAccount.LevelNumber)
				await channel.SendMessageAsync(
					$"{user.Mention} leveled up! Now on level **{userAccount.LevelNumber}**!");
		}

		/// <summary>
		/// Gives a user server points, and gives them a role if they past a certain amount of points
		/// </summary>
		/// <param name="user"></param>
		/// <param name="channel"></param>
		/// <param name="amount"></param>
		public static async void GiveUserServerPoints(SocketGuildUser user, SocketTextChannel channel, uint amount)
		{
			UserAccount userAccount = UserAccountsManager.GetAccount(user);
			userAccount.GetOrCreateServer(user.Guild.Id).Points += amount;

			UserAccountsManager.SaveAccounts();

			//Give the user a role if they have enough points for it.
			ServerList server = ServerListsManager.GetServer(user.Guild);
			ServerRolePoints serverRole =
				server.GetServerRolePoints(userAccount.GetOrCreateServer(user.Guild.Id).Points);
			if (serverRole.PointsRequired == 0) return;

			await user.AddRoleAsync(RoleUtils.GetGuildRole(user.Guild, serverRole.RoleId));

			await channel.SendMessageAsync(
				$"Congrats {user.Mention}, you got {userAccount.GetOrCreateServer(user.Guild.Id).Points} points and got the **{RoleUtils.GetGuildRole(user.Guild, serverRole.RoleId).Name}** role!");
		}
	}
}