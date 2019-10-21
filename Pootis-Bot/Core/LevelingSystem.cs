using Discord.WebSocket;
using Pootis_Bot.Entities;

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
		public static async void UserSentMessage(SocketGuildUser user, SocketTextChannel channel, uint amount)
		{
			UserAccount userAccount = UserAccounts.GetAccount(user);
			uint oldLevel = userAccount.LevelNumber;

			//Nice one EternalClickbait...

			userAccount.Xp += amount;
			UserAccounts.SaveAccounts();

			if (oldLevel != userAccount.LevelNumber)
				await channel.SendMessageAsync($"{user.Mention} leveled up! Now on level **{userAccount.LevelNumber}**!");
		}

		/// <summary>
		/// Gives a user server points, and gives them a role if they past a certain amount of points
		/// </summary>
		/// <param name="user"></param>
		/// <param name="channel"></param>
		/// <param name="amount"></param>
		public static async void GiveUserServerPoints(SocketGuildUser user, SocketTextChannel channel, uint amount)
		{
			UserAccount userAccount = UserAccounts.GetAccount(user);
			userAccount.GetOrCreateServer(user.Guild.Id).Points += amount;

			UserAccounts.SaveAccounts();

			//TODO: Give role if user has enough points for it
		}
	}
}